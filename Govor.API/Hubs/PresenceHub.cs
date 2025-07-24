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
        if (userId == Guid.Empty || await _users.ExistsByIdAsync(userId) == false)
        {
            _logger.LogWarning("User connected with invalid UserId claim.");
            Context.Abort();
            return;
        }

        _onlineUserStore.SetOnlineUser(userId);
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

        var friends = await _scopeService.GetNotifiedUsers(userId);

        foreach (var recipient in friends)
        {
            await Clients.Group(recipient.ToString())
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
                "User disconnected with no exception and invalid UserID claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
            return;
        }
        
        _onlineUserStore.SetOfflineUser(userId);
        
        // Updating was online 
        var user = await  _users.FindByIdAsync(userId);
        user.WasOnline = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        
        var friends = await _scopeService.GetNotifiedUsers(userId);

        foreach (var recipient in friends)
        {
            await Clients.Group(recipient.ToString())
                .SendAsync("UserOffline", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}