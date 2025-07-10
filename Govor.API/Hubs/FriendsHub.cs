using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.Responses.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

public class FriendsHub : Hub
{
    private readonly ILogger<FriendsHub> _logger;
    private readonly IFriendRequestCommandService _friendRequestService;
    private readonly ICurrentUserService _currentUserService;

    public FriendsHub(IFriendRequestCommandService friendRequestService, ICurrentUserService currentUserService, ILogger<FriendsHub> logger)
    {
        _friendRequestService = friendRequestService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

     public async Task<HubResult<object>> SendRequest(Guid targetUserId)
    {
        try
        {
            var userId = _currentUserService.GetCurrentUserId();
            await _friendRequestService.SendAsync(userId, targetUserId);
            await Clients.User(targetUserId.ToString())
                .SendAsync("FriendRequestReceived", userId);

            _logger.LogInformation($"Friend request received for user {targetUserId} from {userId}.");
            return HubResult<object>.Created();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.BadRequest(ex.Message);
        }
        catch (RequestAlreadySentException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.Conflict(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return HubResult<object>.Error("Unexpected error! Please try later!");
        }
    }

    public async Task<HubResult<object>> AcceptRequest(Guid friendshipId)
    {
        try
        {
            var userId = _currentUserService.GetCurrentUserId();
            await _friendRequestService.AcceptAsync(friendshipId, userId);
            await Clients.User(userId.ToString())
                         .SendAsync("FriendRequestAccepted", friendshipId);
            
            _logger.LogInformation($"Friend request accepted for user {userId} from {userId}.");
            return HubResult<object>.Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return HubResult<object>.Error("Unexpected error! Please try later!");
        }
    }

    public async Task<HubResult<object>> RejectRequest(Guid friendshipId)
    {
        try
        {
            var userId = _currentUserService.GetCurrentUserId();
            await _friendRequestService.RejectAsync(friendshipId, userId);
            await Clients.User(userId.ToString())
                         .SendAsync("FriendRequestRejected", friendshipId);
            
            _logger.LogInformation($"Friend request rejected for user {userId} from {userId}.");
            return HubResult<object>.Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return HubResult<object>.Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return HubResult<object>.Error("Unexpected error! Please try later!");
        }
    }
}