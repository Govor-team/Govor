using System.Security.Claims;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize]
public class ChatsHub : Hub
{
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<ChatsHub> _logger;

    public ChatsHub(IUsersRepository usersRepository, ILogger<ChatsHub> logger)
    {
        _usersRepository = usersRepository;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            // Привязываем ConnectionId к UserId
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

    public async Task Edit(string newMessage, Guid messageId)
    {
        
    }
    
    public async Task Send(string message, Guid toUserId)
    {
        // Валидация входных данных
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Empty message received from user {UserId}", GetUserId());
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }
        
        var senderId = GetUserId();
        
        // Проверка существования получателя
        try
        {
            await _usersRepository.FindByIdAsync(toUserId);
        }
        catch (NotFoundByKeyException<User> ex)
        {
            _logger.LogWarning("Recipient user {ToUserId} not found", toUserId);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid recipient userId received from user {UserId}", GetUserId());
            throw;
        }
        
        try
        {
            _logger.LogInformation("Message sent from {SenderId} to {RecipientId}: {Message}", senderId, toUserId, message);

            // Отправка сообщения отправителю и получателю
            await Clients.Group(toUserId.ToString()).SendAsync("Receive", message, senderId);
            await Clients.Group(senderId.ToString()).SendAsync("Receive", message, senderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", senderId, toUserId);
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