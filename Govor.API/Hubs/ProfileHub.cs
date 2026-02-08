using Govor.API.Common.SignalR.Helpers;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize]
public class ProfileHub : Hub
{
    private readonly IGroupsRepository _groupsRepository;
    private readonly IFriendshipService _friendsService;
    private readonly IProfileService _profileService;
    private readonly IHubUserAccessor _userAccessor;
    private readonly ILogger<ProfileHub> _logger;
    private readonly IMediaService _mediaService; 
    
    public ProfileHub(
        IGroupsRepository groupsRepository,
        IFriendshipService friendsService,
        IProfileService profileService,
        IHubUserAccessor userAccessor,
        ILogger<ProfileHub> logger)
    {
        _groupsRepository = groupsRepository;
        _friendsService = friendsService;
        _profileService = profileService;
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId(Context);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        // Friends
        try
        {
            var friendships = await _friendsService.GetFriendsAsync(userId);
            foreach (var friends in friendships)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"friends-{friends.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get friends for user {userId}", userId);
        }
        
        // Groups
        try
        {
            var groups = await _groupsRepository.GetByUserIdAsync(userId);
            foreach (var group in groups)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{group.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get groups for user {userId}", userId);
        }

        _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = _userAccessor.GetUserId(Context);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

        // Friends
        try
        {
            var friendships = await _friendsService.GetFriendsAsync(userId);
            
            foreach (var friendship in friendships)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"friends-{friendship.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove user {userId} from friend groups", userId);
        }

        // Groups
        try
        {
            var groups = await _groupsRepository.GetByUserIdAsync(userId);

            foreach (var group in groups)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-{group.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove user {userId} from groups", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task<HubResult<bool>> SetDescription(string description)
    {
        if(description.Length > 500)
            return HubResult<bool>.Error("The description cannot be longer than 500 characters.");
        
        var userId = _userAccessor.GetUserId(Context);

        try
        {
            await _profileService.SetDescription(description, userId);

            var payload = new { userId, description };

            await NotifyFriendsAndGroupsAsync(userId, "DescriptionUpdated", payload);

            return HubResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the user's {userId} descripton!", userId);
            return HubResult<bool>.Error("An unaccounted error on the server!");
        }
    }

    public async Task<HubResult<bool>> SetAvatar(Guid iconId)
    {
        var userId = _userAccessor.GetUserId(Context);

        try
        {
            if (iconId == Guid.Empty)
                return HubResult<bool>.BadRequest("IconId can't be empty!");

            await _profileService.SetNewIcon(userId, iconId);

            var payload = new { userId, iconId };

            await NotifyFriendsAndGroupsAsync(userId, "AvatarUpdated", payload);

            return HubResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the user's {userId} avatar {iconId}", userId, iconId);
            return HubResult<bool>.Error("An unaccounted error on the server!");
        }
    }
    
    private async Task NotifyFriendsAndGroupsAsync(Guid userId, string eventName, object payload)
    {
        var friendIds = Enumerable.Empty<User>();
        var groups = Enumerable.Empty<ChatGroup>();

        try
        {
            friendIds = await _friendsService.GetFriendsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load friend list for notifications. userId: {userId}", userId);
        }

        try
        {
            groups = await _groupsRepository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load group list for notifications. userId: {userId}", userId);
        }

        var groupIds = groups.Select(g => g.Id).ToList();

        // Current user
        await Clients.Group($"user-{userId}")
            .SendAsync(eventName, payload);

        // Friends
        foreach (var friend in friendIds)
        {
            await Clients.Group($"friends-{friend.Id}")
                .SendAsync(eventName, payload);
        }

        // Groups
        foreach (var groupId in groupIds)
        {
            await Clients.Group($"group-{groupId}")
                .SendAsync(eventName, payload);
        }

        _logger.LogInformation("Sent {EventName} for {UserId} to {Friends} friends and {Groups} groups",
            eventName, userId, friendIds.Count(), groupIds.Count);
    }
}
