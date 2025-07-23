using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
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
    
    public PresenceHub(ILogger<PresenceHub> logger, IUserNotificationScopeService notificationScopeService, IOnlineUserStore onlineUserStore)
    {
        _logger = logger;
        _notificationScopeService = notificationScopeService;
        _onlineUserStore = onlineUserStore;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
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
        var userId = GetUserId();
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
    
    private Guid GetUserId(bool suppressException = false)
    {
        var userIdClaim = Context.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            if (!suppressException)
            {
                _logger.LogError("Could not retrieve sender userId. Claim was: {UserIDClaim}", userIdClaim);
                throw new UnauthorizedAccessException("userId claim is missing or invalid.");
            }

            return Guid.Empty;
        }

        return userId;
    }
}