using Govor.API.Common.SignalR.Helpers;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Messages;
using Govor.Application.Messages.Parameters;
using Govor.Contracts.Requests.SignalR;
using Govor.Contracts.Responses.SignalR;
using Govor.Domain.Models.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize] // api/chats
public class ChatsHub : Hub
{
    private readonly ILogger<ChatsHub> _logger;
    private readonly IMessageSendingService _messageSendingService;
    private readonly IMessageEditingService _messageEditingService;
    private readonly IMessageRemovingService _messageRemovingService;
    private readonly IHubUserAccessor _userAccessor;
    private readonly IChatNotificationService _notifier;
    private readonly IConnectionManager _connectionManager;


    public ChatsHub(ILogger<ChatsHub> logger,
        IMessageSendingService messageSendingService, 
        IMessageEditingService messageEditingService, 
        IHubUserAccessor userAccessor,
        IChatNotificationService notifier,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _messageSendingService = messageSendingService;
        _messageEditingService = messageEditingService;
        _userAccessor = userAccessor;
        _notifier = notifier;
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId(Context);
        if (userId == Guid.Empty)
        {
            Context.Abort();
            return;
        }

        await _connectionManager.OnConnectedAsync(Context.ConnectionId, userId);
        _logger.LogInformation("User {UserId} connected", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userAccessor.GetUserId(Context, true);
        await _connectionManager.OnDisconnectedAsync(Context.ConnectionId, userId);
        
        if (exception != null)
            _logger.LogWarning(exception, "User {UserId} disconnected with error", userId);
        else 
            _logger.LogInformation("User {UserId} disconnected", userId);

        await base.OnDisconnectedAsync(exception);
    }
    
    // --- SEND ---
    public async Task<HubResult<UserMessageResponse>> Send(MessageRequest request)
    {
        return await SafeExecute(async (userId) =>
        {
            ValidateMessageRequest(request);

            var sendParams = MapToSendMessage(request, userId);
            var result = await _messageSendingService.SendMessageAsync(sendParams);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Exception.Message ?? "Failed to send message");

            var response = MapToResponse(result.Message, request.ReplyToMessageId);
            
            await _notifier.NotifyMessageSentAsync(response);

           return HubResult<UserMessageResponse>.Ok(response);
        }, request.RecipientId);
    } 

    // --- REMOVE ---
    public async Task<HubResult<MessageRemovedResponse>> Remove(RemoveMessageRequest request)
    {
        return await SafeExecute(async (userId) =>
        {
            var result = await _messageRemovingService.DeleteMessageAsync(
                new DeleteMessage(
                    userId,
                    request.MessageId,
                    ForceRemove: request.RequestType switch
                    {
                        RemoveMessageRequestType.HideForMe => false,
                        RemoveMessageRequestType.ForceRemove => true,
                        _ => false
                    })
            );

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error.ToString());

            var notification = new MessageRemovedResponse
            {
                MessageId = request.MessageId,
                SenderId = result.Value.SenderId,
                RecipientId = result.Value.RecipientId,
                RecipientType = result.Value.RecipientType
            };

            await _notifier.NotifyMessageRemovedAsync(notification);

            return HubResult<MessageRemovedResponse>.Ok(notification);
        }, request.MessageId);
    }

    // --- EDIT ---
    public async Task<HubResult<MessageEditResponse>> Edit(EditMessageRequest request)
    {
        return await SafeExecute(async (userId) =>
        {
            var editParams = new EditMessage(userId, request.MessageId, request.NewEncryptedContent, DateTime.UtcNow);
            var result = await _messageEditingService.EditMessageAsync(editParams);

            if (!result.IsSuccess || result.OriginalMessage == null)
                throw new InvalidOperationException("Edit message error");

            var response = new MessageEditResponse
            {
                MessageId = result.messageId,
                EditorId = userId,
                RecipientId = result.OriginalMessage.RecipientId,
                RecipientType = result.OriginalMessage.RecipientType,
                NewEncryptedContent = request.NewEncryptedContent,
                EditedAt = editParams.EditedAt,
            };

            await _notifier.NotifyMessageEditedAsync(response);

            return HubResult<MessageEditResponse>.Ok(response);
        }, request.MessageId);
    }
    
    private async Task<HubResult<T>> SafeExecute<T>(Func<Guid, Task<HubResult<T>>> action, Guid targetIdForLog)
    {
        var userId = _userAccessor.GetUserId(Context);
        try
        {
            return await action(userId);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {UserId} -> {TargetId}", userId, targetIdForLog);
            return HubResult<T>.Unauthorized("You are not authorized.");
        }
        catch (FriendshipException)
        {
             return HubResult<T>.Unauthorized("You cannot perform this action due to friendship status.");
        }
        catch (ArgumentException ex)
        {
            return HubResult<T>.BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return HubResult<T>.NotFound("Resource not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing hub method for {UserId}", userId);
            return HubResult<T>.Error("Internal server error");
        }
    }
    
    private void ValidateMessageRequest(MessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EncryptedContent) && 
            (request.MediaAttachments == null || !request.MediaAttachments.Any()))
        {
            throw new ArgumentException("Message must contain content or media.");
        }
        if (request.EncryptedContent.Length > 50_000)
        {
            throw new ArgumentException("Message is too long.");
        }
    }

    private SendMessage MapToSendMessage(MessageRequest request, Guid senderId)
    {
        return new SendMessage(
            EncryptContent: request.EncryptedContent,
            ReplyToMessageId: request.ReplyToMessageId,
            FromUserId: senderId,
            RecipientId: request.RecipientId,
            RecipientType: request.RecipientType,
            SendAt: DateTime.UtcNow,
            Media: request.MediaAttachments?.Select(f => new SendMedia(f.MediaId, f.EncryptedKey)) 
                   ?? Array.Empty<SendMedia>()
        );
    }

    private UserMessageResponse MapToResponse(Message message, Guid? replyToId)
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
}