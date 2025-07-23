using Govor.API.Common.SignalR.Helpers;
using Govor.Application.Interfaces.UserOnlineStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize(Roles = "Admin, User")]
[Route("hubs/presence")]
public class PresenceHub : Hub 
{
    private readonly ILogger<PresenceHub> _logger;
    private readonly IUserNotificationScopeService _notificationScopeService;
    private readonly IOnlineUserStore _onlineUserStore;
    private readonly IHubUserAccessor _userAccessor;

    public PresenceHub(
        ILogger<PresenceHub> logger,
        IUserNotificationScopeService notificationScopeService,
        IOnlineUserStore onlineUserStore,
        IHubUserAccessor userAccessor)
    {
        _logger = logger;
        _notificationScopeService = notificationScopeService;
        _onlineUserStore = onlineUserStore;
        _userAccessor = userAccessor;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId(Context);
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("User connected with invalid UserID claim.");
            Context.Abort();
            return;
        }

        _onlineUserStore.SetOnlineUser(userId);
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

        var friends = await _notificationScopeService.GetNotifiedUsers(userId);

        foreach (var recipient in friends)
        {
            await Clients.Group(recipient.ToString())
                .SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userAccessor.GetUserId(Context, true);
        if (userId == Guid.Empty) return;

        _onlineUserStore.SetOfflineUser(userId);

        var friends = await _notificationScopeService.GetNotifiedUsers(userId);

        foreach (var recipient in friends)
        {
            await Clients.Group(recipient.ToString())
                .SendAsync("UserOffline", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}