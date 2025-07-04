using Govor.API.Services;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Contracts.Requests.SignalR;
using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize]
public class ChatsHub : Hub
{
    private readonly ILogger<ChatsHub> _logger;
    private readonly IChatService _chatService;
    private readonly IGroupService _groupService;
    
    public ChatsHub(ILogger<ChatsHub> logger,
        IChatService chatService
        )
    {
        _logger = logger;
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        
        if (userId != Guid.Empty)
        {
            // Binding ConnectionId to UserId
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation("User {UserId} disconnected", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Remove(Guid recipientId, Guid messageId)
    {
        
    }
    
    public async Task Edit(string newMessage, Guid messageId)
    {
        
    }
    
    public async Task SendGroup(GroupMessageRequest request)
    {
        var senderId = GetUserId();

        // Создание сообщения
        var message = new SendMessage(
            EncryptContent: request.EncryptedContent,
            ReplyToMessageId: request.ReplyToMessageId,
            FromUserId: senderId,
            RecipientId: request.GroupId,
            SendAt: DateTime.UtcNow,
            Media: request.MediaAttachments?.Select(f => new SendMedia(
                f.MediaId, f.EncryptedKey, f.Type, f.MimeType)) ?? Array.Empty<SendMedia>());

        var result = await _groupService.SendMessageAsync(message);
        
        if (!result.IsSuccess)
            throw result.Exception!;

        // Шлём всем участникам группы, кроме отправителя
        await Clients.GroupExcept($"group_{request.GroupId}", Context.ConnectionId)
            .SendAsync("ReceiveGroupMessage", message);

        // Отправителю тоже
        await Clients.Caller.SendAsync("ReceiveGroupMessage", message);
    }
    
    public async Task Send(MessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EncryptedContent))
        {
            _logger.LogWarning("Empty message received from user {UserId}", GetUserId());
            throw new ArgumentException("Message cannot be empty", nameof(request.EncryptedContent));
        }
        
        var senderId = GetUserId();
        
        try
        {
            _logger.LogInformation("Message sent from {SenderId} to {RecipientId} at {UtcNow}", senderId, request.RecipientId, DateTime.UtcNow);
            
            var message = new SendMessage(
                EncryptContent: request.EncryptedContent,
                ReplyToMessageId: request.ReplyToMessageId,
                FromUserId: senderId,
                RecipientId: request.RecipientId,
                SendAt: DateTime.UtcNow,
                Media: request.MediaAttachments?.Select(f => new SendMedia(
                    f.MediaId, f.EncryptedKey, f.Type, f.MimeType)) ?? Array.Empty<SendMedia>());

            Result result = await _chatService.SendMessageAsync(message);
            if(result.IsSuccess == false)
                throw result.Exception;
            
            // Sending a message to the sender and recipient
            await Clients.Group(message.RecipientId.ToString()).SendAsync("Receive", message);
            await Clients.Group(message.FromUserId.ToString()).SendAsync("Receive", message);
            // TODO: Send to Group 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", senderId, request.RecipientId);
            throw;
        }
    }
    
    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst("userID")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError("Could not retrieve sender userId");
            throw new UnauthorizedAccessException("userID claim is missing or invalid");
        }
        return userId;
    }
}