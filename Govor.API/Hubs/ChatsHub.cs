using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Contracts.Requests.SignalR;
using Govor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize]
public class ChatsHub : Hub
{
    private readonly ILogger<ChatsHub> _logger;
    private readonly IMessageService _messageService; // Consolidated service

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
            // Consider whether to throw an exception or just ignore
            // For now, let's throw, as it's likely an issue with the client
            throw new ArgumentException("Message cannot be empty (must have content or attachments).");
        }

        var senderId = GetUserId();
        _logger.LogInformation("Message send initiated by {SenderId} to {RecipientId} of type {RecipientType}", senderId, request.RecipientId, request.RecipientType);

        var sendMessageParams = new SendMessage(
            EncryptContent: request.EncryptedContent,
            ReplyToMessageId: request.ReplyToMessageId,
            FromUserId: senderId,
            RecipientId: request.RecipientId,
            RecipientType: request.RecipientType, // Added RecipientType
            SendAt: DateTime.UtcNow,
            Media: request.MediaAttachments?.Select(f => new SendMedia(
                f.MediaId, f.EncryptedKey, f.Type, f.MimeType)) ?? Array.Empty<SendMedia>()
        );

        try
        {
            var result = await _messageService.SendMessageAsync(sendMessageParams);
            if (!result.IsSuccess || result.MessageId == Guid.Empty)
            {
                _logger.LogError(result.Exception, "Failed to send message from {SenderId} to {RecipientId}. Error: {ErrorMessage}", senderId, request.RecipientId, result.Exception?.Message ?? "Unknown error");
                // It might be better to send an error message back to the caller rather than throwing an exception that disconnects them.
                // For now, rethrow to maintain previous behavior if an exception was present.
                if (result.Exception != null) throw result.Exception;
                throw new HubException("Failed to send message due to an internal error.");
            }

            var messageResponse = new UserMessageResponse // Assuming a response DTO
            {
                MessageId = result.MessageId,
                SenderId = senderId,
                RecipientId = request.RecipientId,
                RecipientType = request.RecipientType,
                EncryptedContent = request.EncryptedContent,
                SentAt = sendMessageParams.SendAt, // Use the time from params
                IsEdited = false,
                MediaAttachments = request.MediaAttachments,
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

            _logger.LogInformation("Message {MessageId} sent successfully from {SenderId} to {RecipientId} ({RecipientType})", result.MessageId, senderId, request.RecipientId, request.RecipientType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", senderId, request.RecipientId);
            // Consider sending a specific error message to the caller instead of a generic HubException or rethrowing.
            // For example: await Clients.Caller.SendAsync("SendMessageFailed", new { Error = ex.Message });
            throw new HubException("An error occurred while sending the message.", ex);
        }
    }

    public async Task Edit(EditMessageRequest request)
    {
        var editorId = GetUserId();
        _logger.LogInformation("Message edit initiated by {EditorId} for message {MessageId}", editorId, request.MessageId);

        if (string.IsNullOrWhiteSpace(request.NewEncryptedContent))
        {
            _logger.LogWarning("Edit request for message {MessageId} by user {EditorId} has empty new content.", request.MessageId, editorId);
            throw new ArgumentException("New message content cannot be empty.", nameof(request.NewEncryptedContent));
        }
        
        var editParams = new EditMessage(
            EditorId: editorId,
            MessageId: request.MessageId,
            NewContent: request.NewEncryptedContent,
            EditedAt: DateTime.UtcNow
        );

        try
        {
            var result = await _messageService.EditMessageAsync(editParams);
            if (!result.IsSuccess)
            {
                _logger.LogError(result.Exception, "Failed to edit message {MessageId} by {EditorId}. Error: {ErrorMessage}", request.MessageId, editorId, result.Exception?.Message ?? "Unknown error");
                if (result.Exception != null) throw result.Exception; // Or specific HubException
                throw new HubException("Failed to edit message.");
            }

            var originalMessage = result.OriginalMessage; // Assuming service returns this
            if (originalMessage == null)
            {
                _logger.LogError("EditMessageAsync succeeded but did not return the original message details for message {MessageId}", request.MessageId);
                throw new HubException("Failed to process message edit due to missing message details.");
            }
            
            var editNotification = new MessageEditedResponse
            {
                MessageId = request.MessageId,
                NewEncryptedContent = request.NewEncryptedContent,
                EditedAt = editParams.EditedAt,
                SenderId = originalMessage.SenderId, // Keep original sender
                RecipientId = originalMessage.RecipientId,
                RecipientType = originalMessage.RecipientType
            };

            // Notify relevant clients
            if (originalMessage.RecipientType == RecipientType.User)
            {
                // Notify sender and recipient
                await Clients.Group(originalMessage.SenderId.ToString()).SendAsync("MessageEdited", editNotification);
                if (originalMessage.SenderId != originalMessage.RecipientId) // Avoid double sending if sender is recipient
                {
                    await Clients.Group(originalMessage.RecipientId.ToString()).SendAsync("MessageEdited", editNotification);
                }
            }
            else if (originalMessage.RecipientType == RecipientType.Group)
            {
                await Clients.Group($"group_{originalMessage.RecipientId}").SendAsync("MessageEdited", editNotification);
            }
            _logger.LogInformation("Message {MessageId} edited successfully by {EditorId}", request.MessageId, editorId);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to edit message {MessageId} by user {EditorId}", request.MessageId, editorId);
            throw new HubException("You are not authorized to edit this message.", ex);
        }
        catch (KeyNotFoundException ex) // Or a custom NotFoundException
        {
            _logger.LogWarning(ex, "Attempt to edit non-existent message {MessageId} by user {EditorId}", request.MessageId, editorId);
            throw new HubException("Message not found.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId} by {EditorId}", request.MessageId, editorId);
            throw new HubException("An error occurred while editing the message.", ex);
        }
    }

    public async Task Remove(RemoveMessageRequest request)
    {
        var removerId = GetUserId();
        _logger.LogInformation("Message removal initiated by {RemoverId} for message {MessageId}", removerId, request.MessageId);

        var removeParams = new DeleteMessage(
            DeleterId: removerId,
            MessageId: request.MessageId
        );

        try
        {
            var result = await _messageService.DeleteMessageAsync(removeParams);
            if (!result.IsSuccess)
            {
                _logger.LogError(result.Exception, "Failed to remove message {MessageId} by {RemoverId}. Error: {ErrorMessage}", request.MessageId, removerId, result.Exception?.Message ?? "Unknown error");
                if (result.Exception != null) throw result.Exception;
                throw new HubException("Failed to remove message.");
            }
            
            var originalMessage = result.OriginalMessage; // Assuming service returns this
             if (originalMessage == null)
            {
                _logger.LogError("DeleteMessageAsync succeeded but did not return the original message details for message {MessageId}", request.MessageId);
                throw new HubException("Failed to process message deletion due to missing message details.");
            }

            var removalNotification = new MessageRemovedResponse
            {
                MessageId = request.MessageId,
                SenderId = originalMessage.SenderId,
                RecipientId = originalMessage.RecipientId,
                RecipientType = originalMessage.RecipientType
            };

            // Notify relevant clients
            if (originalMessage.RecipientType == RecipientType.User)
            {
                await Clients.Group(originalMessage.SenderId.ToString()).SendAsync("MessageRemoved", removalNotification);
                if (originalMessage.SenderId != originalMessage.RecipientId)
                {
                    await Clients.Group(originalMessage.RecipientId.ToString()).SendAsync("MessageRemoved", removalNotification);
                }
            }
            else if (originalMessage.RecipientType == RecipientType.Group)
            {
                await Clients.Group($"group_{originalMessage.RecipientId}").SendAsync("MessageRemoved", removalNotification);
            }
            _logger.LogInformation("Message {MessageId} removed successfully by {RemoverId}", request.MessageId, removerId);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to remove message {MessageId} by user {RemoverId}", request.MessageId, removerId);
            throw new HubException("You are not authorized to remove this message.", ex);
        }
        catch (KeyNotFoundException ex) // Or a custom NotFoundException
        {
            _logger.LogWarning(ex, "Attempt to remove non-existent message {MessageId} by user {RemoverId}", request.MessageId, removerId);
            throw new HubException("Message not found.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing message {MessageId} by {RemoverId}", request.MessageId, removerId);
            throw new HubException("An error occurred while removing the message.", ex);
        }
    }
    
    private Guid GetUserId(bool suppressException = false)
    {
        var userIdClaim = Context.User?.FindFirst("userID")?.Value;
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