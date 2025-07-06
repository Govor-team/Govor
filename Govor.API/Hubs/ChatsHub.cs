using Govor.API.Services;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Contracts.Requests.SignalR;
using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize] // api/chats
public class ChatsHub : Hub
{
    private readonly ILogger<ChatsHub> _logger;
    private readonly IMessageService _messageService;
    
    public ChatsHub(ILogger<ChatsHub> logger, IMessageService messageService)
    {
        _logger = logger;
        _messageService = messageService;
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
        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId} and added to their group", userId, Context.ConnectionId);

        // TODO: Add user to their chat groups - this might require fetching user's groups from a service
        // var userGroups = await _userService.GetUserGroupsAsync(userId);
        // foreach (var group in userGroups)
        // {
        //     await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{group.Id}");
        // }

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId(suppressException: true); // Suppress exception if userID is not found (e.g. connection aborted early)
        if (userId != Guid.Empty)
        {
            // Remove user from their own group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation("User {UserId} disconnected with ConnectionId {ConnectionId} and removed from their group", userId, Context.ConnectionId);

            // TODO: Remove user from their chat groups
            // var userGroups = await _userService.GetUserGroupsAsync(userId); // This might be problematic if the service relies on the user being connected
            // foreach (var group in userGroups)
            // {
            //     await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{group.Id}");
            // }
        }
        else if (exception != null)
        {
            _logger.LogWarning(exception, "User disconnected with an exception and invalid UserID claim. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("User disconnected with no exception and invalid UserID claim. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Send(MessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EncryptedContent) && (request.MediaAttachments == null || !request.MediaAttachments.Any()))
        {
            _logger.LogWarning("Empty message (no content and no attachments) received from user {UserId}", GetUserId());
            throw new ArgumentException("Message cannot be empty (must have content or attachments).");
        }

        var senderId = GetUserId();
        _logger.LogInformation("Message send initiated by {SenderId} to {RecipientId} of type {RecipientType}", senderId, request.RecipientId, request.RecipientType);

        var sendMessageParams = new SendMessage(
            EncryptContent: request.EncryptedContent,
            ReplyToMessageId: request.ReplyToMessageId,
            FromUserId: senderId,
            RecipientId: request.RecipientId,
            RecipientType: request.RecipientType,
            SendAt: DateTime.UtcNow,
            Media: request.MediaAttachments?.Select(f => new SendMedia(
                f.MediaId, f.EncryptedKey)) ?? Array.Empty<SendMedia>()
        );

        try
        {
            var result = await _messageService.SendMessageAsync(sendMessageParams);
            
            if (!result.IsSuccess || result.Message.Id == Guid.Empty)
            {
                _logger.LogError(result.Exception, "Failed to send message from {SenderId} to {RecipientId}. Error: {ErrorMessage}", senderId, request.RecipientId, result.Exception?.Message ?? "Unknown error");
                if (result.Exception != null) throw result.Exception;
                throw new HubException("Failed to send message due to an internal error.");
            }

            var messageResponse = new UserMessageResponse // Assuming a response DTO
            {
                MessageId = result.Message.Id,
                SenderId = result.Message.SenderId,
                RecipientId = result.Message.RecipientId,
                RecipientType = result.Message.RecipientType,
                EncryptedContent = result.Message.EncryptedContent,
                SentAt = result.Message.SentAt, 
                IsEdited = false,
                MediaAttachments = result.Message.MediaAttachments
                    .Select(m => m.MediaFile).ToList(),
                ReplyToMessageId = request.ReplyToMessageId
            };

            // Notify recipient (user or group)
            if (request.RecipientType == RecipientType.User)
            {
                // Send to the recipient's personal group
                await Clients.Group(request.RecipientId.ToString()).SendAsync("ReceiveMessage", messageResponse);
            }
            else if (request.RecipientType == RecipientType.Group)
            {
                // Send to all members of the group, including the sender if they are part of the group via a different connection
                await Clients.Group($"group_{request.RecipientId}").SendAsync("ReceiveMessage", messageResponse);
            }

            // Notify sender (confirmation) on their connection
            await Clients.Caller.SendAsync("MessageSent", messageResponse); // Or use "ReceiveMessage" if the sender should also just get it like anyone else

            _logger.LogInformation("Message {MessageId} sent successfully from {SenderId} to {RecipientId} ({RecipientType})", result.Message.Id, senderId, request.RecipientId, request.RecipientType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", senderId, request.RecipientId);
            // Consider sending a specific error message to the caller instead of a generic HubException or rethrowing.
            // For example: await Clients.Caller.SendAsync("SendMessageFailed", new { Error = ex.Message });
            throw new HubException("An error occurred while sending the message.", ex);
        }
    }

    public async Task Remove(RemoveMessageRequest request)
    {
        
    }
    
    public async Task Edit(EditMessageRequest request)
    {
        
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