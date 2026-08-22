using AutoMapper;
using Govor.API.Common.SignalR.Helpers;
using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Friends;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

public class FriendsHub : Hub
{
    private readonly ILogger<FriendsHub> _logger;
    private readonly IFriendRequestCommandService _friendRequestService;
    private readonly IHubUserAccessor _userAccessor;
    private readonly IMapper _mapper;

    public FriendsHub(
        ILogger<FriendsHub> logger,
        IFriendRequestCommandService friendRequestService,
        IHubUserAccessor userAccessor,
        IMapper mapper)
    {
        _logger = logger;
        _friendRequestService = friendRequestService;
        _userAccessor = userAccessor;
        _mapper = mapper;
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
        
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId} and added to their group",
            userId, Context.ConnectionId);
        

        await base.OnConnectedAsync();
    } 
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userAccessor.GetUserId(Context, true);
        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            _logger.LogInformation(
                "User {UserId} disconnected with ConnectionId {ConnectionId} and removed from their group", userId,
                Context.ConnectionId);
        }
        else if (exception != null)
        {
            _logger.LogWarning(exception,
                "User disconnected with an exception and invalid UserId claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "User disconnected with no exception and invalid UserId claim. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
     
    public async Task<HubResult<object>> SendRequest(Guid targetUserId) 
    {
        try
        {
            var userId = _userAccessor.GetUserId(Context);
            var result = await _friendRequestService.SendAsync(userId, targetUserId);
           
            if(result.IsFailure)
                return HubResult<object>.Error(result.Error.ToString());
            
            var friendship = result.Value;
            var dto = _mapper.Map<FriendshipDto>(friendship);
            
            await Clients.Group(targetUserId.ToString())
                .SendAsync("FriendRequestReceived", dto);
            
            await Clients.Group(userId.ToString())
                .SendAsync("YourFriendRequestReceived", dto);
            
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
            var userId = _userAccessor.GetUserId(Context);
            var result = await _friendRequestService.AcceptAsync(friendshipId, userId);
            
            if(result.IsFailure)
                return HubResult<object>.BadRequest(result.Error.ToString());
            
            var friendship = result.Value;
            
            var dto = _mapper.Map<FriendshipDto>(friendship);
            
            await Clients.Group(userId.ToString())
                .SendAsync("FriendRequestAccepted", dto);

            await Clients.Group(friendship.RequesterId.ToString())
                .SendAsync( "YourFriendRequestAccepted", dto);

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
            var userId = _userAccessor.GetUserId(Context);
            var result = await _friendRequestService.RejectAsync(friendshipId, userId);
            
            if(result.IsFailure)
                return HubResult<object>.BadRequest(result.Error.ToString());
            
            var friendship = result.Value;
            
            var dto = _mapper.Map<FriendshipDto>(friendship);

            await Clients.Group(userId.ToString())
                         .SendAsync("FriendRequestRejected", dto);

            await Clients.Group(friendship.RequesterId.ToString())
                         .SendAsync("YourFriendRequestRejected", dto);

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