using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Contracts.Requests.SignalR;
using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize] // api/chats
public class ChatsHub : Hub
{
    private readonly ILogger<ChatsHub> _logger;
    private readonly IMessageCommandService _messageCommandService;
    private readonly IUserGroupsService _userService;

    public ChatsHub(ILogger<ChatsHub> logger, IMessageCommandService messageCommandService, IUserGroupsService userService)
    {
        _logger = logger;
        _messageCommandService = messageCommandService;
        _userService = userService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("User connected with invalid UserID claim.");
            Context.Abort(); // Abort connection if userID is invalid
            return;
        }

        // Add user to their own group (for private messages and notifications)
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId} and added to their group",
            userId, Context.ConnectionId);

        var userGroups = await _userService.GetUserGroupsAsync(userId);
        foreach (var group in userGroups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{group.Id}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId =
            GetUserId(suppressException: true); // Suppress exception if userID is not found (e.g. connection aborted early)
        if (userId != Guid.Empty)
        {
            // Remove user from their own group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation(
                "User {UserId} disconnected with ConnectionId {ConnectionId} and removed from their group", userId,
                Context.ConnectionId);

            var userGroups =
                await _userService
                    .GetUserGroupsAsync(
                        userId); // This might be problematic if the service relies on the user being connected
            foreach (var group in userGroups)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{group.Id}");
            }
        }
        else if (exception != null)
        {
            _logger.LogWarning(exception,
                "User disconnected with an exception and invalid UserID claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "User disconnected with no exception and invalid UserID claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<HubResult<UserMessageResponse>> Send(MessageRequest request)
    {
        var senderId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.EncryptedContent) &&
            (request.MediaAttachments == null || !request.MediaAttachments.Any()))
        {
            _logger.LogWarning("Empty message received from user {UserId}", senderId);
            return HubResult<UserMessageResponse>.BadRequest("Message must contain content or media.");
        }

        _logger.LogInformation("Sending message from {SenderId} to {RecipientId} ({RecipientType})",
            senderId, request.RecipientId, request.RecipientType);

        var sendMessageParams = new SendMessage(
            EncryptContent: request.EncryptedContent,
            ReplyToMessageId: request.ReplyToMessageId,
            FromUserId: senderId,
            RecipientId: request.RecipientId,
            RecipientType: request.RecipientType,
            SendAt: DateTime.UtcNow,
            Media: request.MediaAttachments?.Select(f => new SendMedia(f.MediaId, f.EncryptedKey)) ??
                   Array.Empty<SendMedia>()
        );

        try
        {
            var result = await _messageCommandService.SendMessageAsync(sendMessageParams);

            if (!result.IsSuccess || result.Message.Id == Guid.Empty)
                return LogAndError<UserMessageResponse>(senderId, request.RecipientId, "Failed to send message", result.Exception);

            var response = BuildUserMessageResponse(result.Message, request.ReplyToMessageId);

            await NotifyClientsAboutMessage(response);

            return HubResult<UserMessageResponse>.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return LogAndUnauthorized<UserMessageResponse>(ex, "Unauthorized sending attempt", senderId,
                request.RecipientId);
        }
        catch (FriendshipException)
        {
            return HubResult<UserMessageResponse>.Unauthorized(
                "You cannot send this message because you are not friends.");
        }
        catch (ArgumentException)
        {
            return HubResult<UserMessageResponse>.BadRequest("Invalid message content.");
        }
        catch (Exception ex)
        {
            return LogAndError<UserMessageResponse>(senderId, request.RecipientId, "Unhandled exception", ex);
        }
    }

    
    public async Task<HubResult<MessageRemovedResponse>> Remove(RemoveMessageRequest request)
    {
        var removerId = GetUserId();
        _logger.LogInformation("Removing message {MessageId} by user {RemoverId}", request.MessageId, removerId);

        try
        {
            var result = await _messageCommandService.DeleteMessageAsync(new DeleteMessage(removerId, request.MessageId));

            if (!result.IsSuccess || result.OriginalMessage == null)
                return LogAndError<MessageRemovedResponse>(removerId, request.MessageId, "Message deletion failed", result.Exception);

            var notification = new MessageRemovedResponse
            {
                MessageId = request.MessageId,
                SenderId = result.OriginalMessage.SenderId,
                RecipientId = result.OriginalMessage.RecipientId,
                RecipientType = result.OriginalMessage.RecipientType
            };

            await NotifyClientsAboutRemoval(notification);

            _logger.LogInformation("Message {MessageId} removed successfully by {RemoverId}", request.MessageId, removerId);
            return HubResult<MessageRemovedResponse>.Ok(notification);
        }
        catch (UnauthorizedAccessException ex)
        {
            return LogAndUnauthorized<MessageRemovedResponse>(ex, "Unauthorized removal", removerId, request.MessageId);
        }
        catch (KeyNotFoundException ex)
        {
            return LogAndNotFound<MessageRemovedResponse>(ex, "Message not found", removerId, request.MessageId);
        }
        catch (Exception ex)
        {
            return LogAndError<MessageRemovedResponse>(removerId, request.MessageId, "Unhandled deletion error", ex);
        }
    }


    public async Task<HubResult<MessageEditResponse>> Edit(EditMessageRequest request)
    {
        var editor = GetUserId();
        _logger.LogInformation("Editing message {MessageId} by user {EditorId}", request.MessageId, editor);

        var editMessageParam = new EditMessage(editor,
            request.MessageId,
            request.NewEncryptedContent,
            DateTime.UtcNow);

        try
        {
            var result = await _messageCommandService.EditMessageAsync(editMessageParam);

            if (!result.IsSuccess || result.OriginalMessage == null)
                return LogAndError<MessageEditResponse>(editor, request.MessageId, "Edit message error",
                    result.Exception);

            var response = new MessageEditResponse()
            {
                MessageId = result.messageId,
                EditorId = editor,
                RecipientId = result.OriginalMessage.RecipientId,
                RecipientType = result.OriginalMessage.RecipientType,
                NewEncryptedContent = request.NewEncryptedContent,
                EditedAt = editMessageParam.EditedAt,
            };
            
            await NotifyClientsAboutEdit(response);
            
            _logger.LogInformation("Message {MessageId} edited successfully by {editor}", request.MessageId, editor);
            
            return HubResult<MessageEditResponse>.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return LogAndUnauthorized<MessageEditResponse>(ex, "Unauthorized editing", editor, request.MessageId);
        }
        catch (KeyNotFoundException ex)
        {
            return LogAndNotFound<MessageEditResponse>(ex, "Message not found", editor, request.MessageId);
        }
        catch (Exception ex)
        {
            return LogAndError<MessageEditResponse>(editor, request.MessageId, "Unhandled exception error", ex);
        }
    }


    private UserMessageResponse BuildUserMessageResponse(Message message, Guid? replyToId)
    {
        return new UserMessageResponse
        {
            MessageId = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            RecipientType = message.RecipientType,
            EncryptedContent = message.EncryptedContent,
            SentAt = message.SentAt,
            IsEdited = false,
            MediaAttachments = message.MediaAttachments.Select(m => m.MediaFile).ToList(),
            ReplyToMessageId = replyToId
        };
    }

    private async Task NotifyClientsAboutMessage(UserMessageResponse response)
    {
        string group = response.RecipientType == RecipientType.User
            ? response.RecipientId.ToString()
            : $"group_{response.RecipientId}";

        await Clients.Group(group).SendAsync("ReceiveMessage", response);
        await Clients.Caller.SendAsync("MessageSent", response);
    }

    private async Task NotifyClientsAboutRemoval(MessageRemovedResponse response)
    {
        if (response.RecipientType == RecipientType.User)
        {
            await Clients.Group(response.SenderId.ToString()).SendAsync("MessageRemoved", response);
            if (response.SenderId != response.RecipientId)
                await Clients.Group(response.RecipientId.ToString()).SendAsync("MessageRemoved", response);
        }
        else
        {
            await Clients.Group($"group_{response.RecipientId}").SendAsync("MessageRemoved", response);
        }
    }

    private async Task NotifyClientsAboutEdit(MessageEditResponse response)
    {
        if (response.RecipientType == RecipientType.User)
        {
            await Clients.Group(response.EditorId.ToString()).SendAsync("MessageEdit", response);
            if (response.EditorId != response.RecipientId)
                await Clients.Group(response.RecipientId.ToString()).SendAsync("MessageEdit", response);
        }
        else
        {
            await Clients.Group($"group_{response.RecipientId}").SendAsync("MessageEdit", response);
        }
    }
    
    // Logging helpers
    private HubResult<T> LogAndError<T>(Guid userId, Guid targetId, string message, Exception ex)
    {
        _logger.LogError(ex, "{Message} from {UserId} to {TargetId}", message, userId, targetId);
        return HubResult<T>.Error(ex?.Message ?? "Internal server error");
    }

    private HubResult<T> LogAndUnauthorized<T>(Exception ex, string msg, Guid userId, Guid targetId)
    {
        _logger.LogWarning(ex, "{Msg}: {UserId} -> {TargetId}", msg, userId, targetId);
        return HubResult<T>.Unauthorized("You are not authorized to perform this action.");
    }

    private HubResult<T> LogAndNotFound<T>(Exception ex, string msg, Guid userId, Guid targetId)
    {
        _logger.LogWarning(ex, "{Msg}: {UserId} -> {TargetId}", msg, userId, targetId);
        return HubResult<T>.NotFound("Message not found.");
    }


    private Guid GetUserId(bool suppressException = false)
    {
        var userIdClaim = Context.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            if (!suppressException)
            {
                _logger.LogError("Could not retrieve sender userId. Claim was: {UserIDClaim}", userIdClaim);
                throw new UnauthorizedAccessException("userID claim is missing or invalid.");
            }

            return Guid.Empty;
        }

        return userId;
    }
}