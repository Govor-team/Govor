using Govor.API.Common.SignalR.Helpers;
using Govor.Application.Interfaces.UserOnlineStatus;
using Govor.Core.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize(Roles = "Admin, User")]
[Route("hubs/presence")]
public class PresenceHub : Hub 
{
    private readonly ILogger<PresenceHub> _logger;
    private readonly IUserNotificationScopeService _scopeService;
    private readonly IOnlineUserStore _onlineUserStore;
    private readonly IHubUserAccessor _userAccessor;
    private readonly IUsersRepository _users;
    
    public PresenceHub(
        ILogger<PresenceHub> logger,
        IUserNotificationScopeService scopeService,
        IOnlineUserStore onlineUserStore,
        IHubUserAccessor userAccessor,
        IUsersRepository users)
    {
        _logger = logger;
        _users = users;
        _scopeService = scopeService;
        _onlineUserStore = onlineUserStore;
        _userAccessor = userAccessor;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId(Context);
        
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("User connected with invalid UserId claim.");
            Context.Abort();
            return;
        }
        
        var isFirstConnection = _onlineUserStore.AddConnection(userId, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

        if (isFirstConnection)
        {
            var friends = await _scopeService.GetNotifiedUsers(userId);
            await Clients.Groups(friends.Select(f => f.ToString()).ToList())
                .SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = _userAccessor.GetUserId(Context, true);
        
        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation(
                "User {UserId} disconnected with ConnectionId {ConnectionId} and removed from their group", userId,
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "User disconnected with no exception and invalid UserId claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
            return;
        }
        
        var isLastConnection = _onlineUserStore.RemoveConnection(userId, Context.ConnectionId);
        
        if (isLastConnection)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                
                var currentConnections = _onlineUserStore.GetConnections(userId);
                if (currentConnections == null || !currentConnections.Any())
                {
                    var friends = await _scopeService.GetNotifiedUsers(userId);
                    await Clients.Groups(friends.Select(f => f.ToString()).ToList())
                        .SendAsync("UserOffline", userId);
                    
                    try 
                    {
                        var user = await _users.FindByIdAsync(userId);
                        if (user != null)
                        {
                            user.WasOnline = DateTime.UtcNow;
                            await _users.UpdateAsync(user);
                        }
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex, "Error updating WasOnline for {UserId}", userId);
                    }
                }
            });
        }

        await base.OnDisconnectedAsync(exception);
    }
}