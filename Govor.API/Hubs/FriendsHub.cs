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
    
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("User connected with invalid UserID claim.");
            Context.Abort(); // Abort connection if userID is invalid
            return;
        }

        // Add user to their own group (for private messages and notifications)
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId} and added to their group",
            userId, Context.ConnectionId);
        

        await base.OnConnectedAsync();
    } 
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId =
            GetUserId(suppressException: true); // Suppress exception if userID is not found (e.g. connection aborted early)
        if (userId != Guid.Empty)
        {
            // Remove user from their own group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation(
                "User {UserId} disconnected with ConnectionId {ConnectionId} and removed from their group", userId,
                Context.ConnectionId);
        }
        else if (exception != null)
        {
            _logger.LogWarning(exception,
                "User disconnected with an exception and invalid UserID claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "User disconnected with no exception and invalid UserID claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
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
    
    private Guid GetUserId(bool suppressException = false)
    {
        var userIdClaim = Context.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            if (!suppressException)
            {
                _logger.LogError("Could not retrieve sender userId. Claim was: {UserIDClaim}", userIdClaim);
                throw new UnauthorizedAccessException("userID claim is missing or invalid.");
            }

            return Guid.Empty;
        }

        return userId;
    }
}