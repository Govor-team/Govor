using Govor.API.Services;
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

            if (request.RecipientType == RecipientType.User)
            {
                await SendUser(message);
            }
            // TODO: Send to Group 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", senderId, request.RecipientId);
            throw;
        }
    }
    
    private async Task SendUser(SendMessage sendMessage)
    {
        Result result = await _chatService.SendMessageAsync(sendMessage);
        if(result.IsSuccess == false)
            throw result.Exception;
        
        // Sending a message to the sender and recipient
        await Clients.Group(sendMessage.RecipientId.ToString()).SendAsync("Receive", sendMessage);
        await Clients.Group(sendMessage.FromUserId.ToString()).SendAsync("Receive", sendMessage);
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