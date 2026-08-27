using Govor.API.Common.SignalR.Helpers;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Infrastructure.Common;
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
    private readonly IMessageReadingService _messageReadingService;
    private readonly IMessageSendingService _messageSendingService;
    private readonly IMessageEditingService _messageEditingService;
    private readonly IMessageRemovingService _messageRemovingService;
    private readonly IHubUserAccessor _userAccessor;
    private readonly IChatNotificationService _notifier;
    private readonly IConnectionManager _connectionManager;
    private readonly INowDateTimeProvider _nowDateTimeProvider;

    public ChatsHub(ILogger<ChatsHub> logger,
        IMessageReadingService messageReadingService,
        IMessageSendingService messageSendingService, 
        IMessageEditingService messageEditingService, 
        IMessageRemovingService messageRemovingService,
        IHubUserAccessor userAccessor,
        IChatNotificationService notifier,
        INowDateTimeProvider nowDateTimeProvider,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _nowDateTimeProvider = nowDateTimeProvider;
        _messageSendingService = messageSendingService;
        _messageEditingService = messageEditingService;
        _messageRemovingService = messageRemovingService;
        _messageReadingService = messageReadingService;
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
    // --- Read ---
    public async Task<HubResult<MessageReadResponse>> Read(ReadMessageRequest request)
    {
        return await SafeExecute(async (userId) =>
        {
           var result = await _messageReadingService.ReadMessageAsync(userId, request.MessageId);
           if(!result.IsSuccess)
               throw new InvalidOperationException(result.Error.ToString());
           
           var message = result.Value;
           
           var msgv = message.MessageViews.First(v => v.UserId == userId);
           
           var response = new MessageReadResponse()
           {
               ViewId = msgv.Id,
               MessageId = request.MessageId,
               ReaderId = userId,
               WhenWas = msgv.ViewedAt,
               RecipientId = message.RecipientId,
               RecipientType = message.RecipientType,
           };
           
           await _notifier.NotifyMessageWasReadAsync(response);
           
           return HubResult<MessageReadResponse>.Ok(response);
        }, request.MessageId);
    }
    // --- REMOVE ---
    public async Task<HubResult<MessageRemovedResponse>> Remove(RemoveMessageRequest request)
    {
        return await SafeExecute(async (userId) =>
        {
            var deletemessage = new DeleteMessage(
                    userId,
                    request.MessageId,
                    ForceRemove: request.RequestType switch
                    {
                        RemoveMessageRequestType.HideForMe => false,
                        RemoveMessageRequestType.ForceRemove => true,
                        _ => false
                    }
                );
            
            var result = await _messageRemovingService.DeleteMessageAsync(deletemessage);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error.ToString());

            var notification = new MessageRemovedResponse
            {
                MessageId = request.MessageId,
                SenderId = result.Value.SenderId,
                RecipientId = result.Value.RecipientId, // private chat id or group id 
                RequestType = request.RequestType,
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
            var editParams = new EditMessage(
                    userId, 
                    request.MessageId,
                    request.NewEncryptedContent, 
                    _nowDateTimeProvider.Now);
            
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
            SendAt: _nowDateTimeProvider.Now,
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