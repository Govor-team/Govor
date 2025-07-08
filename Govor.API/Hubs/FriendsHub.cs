using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

public class FriendsHub : Hub
{
    private readonly IFriendRequestService _friendRequestService;
    private readonly ICurrentUserService _currentUserService;

    public FriendsHub(IFriendRequestService friendRequestService, ICurrentUserService currentUserService)
    {
        _friendRequestService = friendRequestService;
        _currentUserService = currentUserService;
    }

    public async Task SendRequest(Guid targetUserId)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _friendRequestService.SendFriendRequestAsync(userId, targetUserId);
        await Clients.User(targetUserId.ToString()).SendAsync("FriendRequestReceived", userId);
    }

    public async Task AcceptRequest(Guid friendshipId)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _friendRequestService.AcceptFriendRequestAsync(friendshipId, userId);
        await Clients.User(userId.ToString()).SendAsync("FriendRequestAccepted", friendshipId);
    }

    public async Task RejectRequest(Guid friendshipId)
    {
        var userId = _currentUserService.GetCurrentUserId();
        await _friendRequestService.RejectFriendRequestAsync(friendshipId, userId);
        await Clients.User(userId.ToString()).SendAsync("FriendRequestRejected", friendshipId);
    }
}