using Govor.API.Common.SignalR.Helpers;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Medias;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses.SignalR;
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
    private readonly ISynchingService _synchingService;
    private readonly ILogger<ProfileHub> _logger;
    private readonly IMediaService _mediaService; 
    
    public ProfileHub(
        IGroupsRepository groupsRepository,
        IFriendshipService friendsService,
        IProfileService profileService,
        IHubUserAccessor userAccessor,
        ISynchingService synchingService,
        IMediaService mediaService,
        ILogger<ProfileHub> logger)
    {
        _groupsRepository = groupsRepository;
        _friendsService = friendsService;
        _profileService = profileService;
        _userAccessor = userAccessor;
        _synchingService = synchingService;
        _mediaService = mediaService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId(Context);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = _userAccessor.GetUserId(Context);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task<HubResult<bool>> SetDescription(string description)
    {
        if (description.Length > 500)
            return HubResult<bool>.Error("Description length exceeded.");

        var userId = _userAccessor.GetUserId(Context);

        try
        {
            description = _synchingService.NormalizeNewlines(description);
            await _profileService.SetDescription(description, userId);

            var payload = new DescriptionUpdatePayload
            {
                UserId = userId,
                Description = description
            };

            await NotifyProfileUpdatedAsync(userId, "DescriptionUpdated", payload);

            return HubResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update description for user {UserId}", userId);
            return HubResult<bool>.Error("Server error.");
        }
    }

    public async Task<HubResult<bool>> SetAvatar(Guid iconId)
    {
        var userId = _userAccessor.GetUserId(Context);

        try
        {
            if (iconId == Guid.Empty || !await _mediaService.HasMediaAsync(iconId))
                return HubResult<bool>.BadRequest("Invalid icon id.");

            await _profileService.SetNewIcon(userId, iconId);

            var payload = new AvatarUpdatePayload(){UserId = userId, IconId = iconId};

            await NotifyProfileUpdatedAsync(userId, "AvatarUpdated", payload);

            return HubResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the user's {userId} avatar {iconId}", userId, iconId);
            return HubResult<bool>.Error("An unaccounted error on the server!");
        }
    }
    
    private async Task NotifyProfileUpdatedAsync(
        Guid UserId,
        string eventName,
        object payload)
    {
        try
        {
            var recipients = new HashSet<Guid>();

            // 1. Friends
            try
            {
                var friends = await _friendsService.GetFriendsAsync(UserId);
                recipients.UnionWith(friends.Select(f => f.Id));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User has no friends for user {userId}", UserId);
            }
            
            // 2. (pending)
            try
            {
                var potentialFriends = await _friendsService.GetPotentialFriendsAsync(UserId);
                recipients.UnionWith(potentialFriends.Select(p => p.Id));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User has potential friends for user {userId}", UserId);
            }


            // 3. groups
            /*var groups = await _groupsRepository.GetByUserIdAsync(authorUserId);
            foreach (var group in groups)
            {
                var members =
                    await _groupsRepository.Get(group.Id);

                recipients.UnionWith(members.Select(m => m.Id));
            }*/
            
            recipients.Add(UserId);

            // 5. 
            // recipients.RemoveWhere(id =>
            //     !_profileVisibilityService.CanViewProfile(id, authorUserId));

            
            // 6.  1 user = 1 event
            var recipientsStrings = 
                recipients.Select(r => $"user:{r}").ToList();
            
            foreach (var recipientId in recipients)
            {
                await Clients.Groups(recipientsStrings)
                    .SendAsync(eventName, payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to notify profile update for user {UserId}",
                UserId);
        }
    }
}
