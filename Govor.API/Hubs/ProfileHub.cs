using Govor.API.Common.SignalR.Helpers;
using Govor.Application.Friends;
using Govor.Application.Medias;
using Govor.Application.Profiles;
using Govor.Application.Synching;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs;

[Authorize]
public sealed class ProfileHub : Hub
{
    private const string UserGroupPrefix = "user:";

    private const string DescriptionUpdatedEvent = "DescriptionUpdated";
    private const string AvatarUpdatedEvent = "AvatarUpdated";

    private readonly IFriendshipService _friendshipService;
    private readonly IProfileService _profileService;
    private readonly IHubUserAccessor _userAccessor;
    private readonly ISynchingService _synchingService;
    private readonly IMediaService _mediaService;
    private readonly ILogger<ProfileHub> _logger;

    public ProfileHub(
        IFriendshipService friendshipService,
        IProfileService profileService,
        IHubUserAccessor userAccessor,
        ISynchingService synchingService,
        IMediaService mediaService,
        ILogger<ProfileHub> logger)
    {
        _friendshipService = friendshipService;
        _profileService = profileService;
        _userAccessor = userAccessor;
        _synchingService = synchingService;
        _mediaService = mediaService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId(Context);
        var groupName = GetUserGroup(userId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupName);

        _logger.LogDebug(
            "User {UserId} connected to ProfileHub with connection {ConnectionId}",
            userId,
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userAccessor.GetUserId(Context);
        var groupName = GetUserGroup(userId);

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            groupName);

        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "User {UserId} disconnected from ProfileHub with connection {ConnectionId}",
                userId,
                Context.ConnectionId);
        }
        else
        {
            _logger.LogDebug(
                "User {UserId} disconnected from ProfileHub with connection {ConnectionId}",
                userId,
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<HubResult<bool>> SetDescription(string? description)
    {
        if (description is null)
            return HubResult<bool>.BadRequest("Description cannot be null.");

        description = _synchingService.NormalizeNewlines(description);

        if (description.Length > 500)
            return HubResult<bool>.BadRequest(
                "Description length exceeded.");

        var userId = _userAccessor.GetUserId(Context);

        try
        {
            await _profileService.SetDescription(
                description,
                userId);

            var payload = new DescriptionUpdatePayload
            {
                UserId = userId,
                Description = description
            };

            await NotifyProfileUpdatedAsync(
                userId,
                DescriptionUpdatedEvent,
                payload);

            return HubResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update description for user {UserId}",
                userId);

            return HubResult<bool>.Error(
                "Server error.");
        }
    }

    public async Task<HubResult<bool>> SetAvatar(Guid iconId)
    {
        if (iconId == Guid.Empty)
            return HubResult<bool>.BadRequest(
                "Invalid icon id.");

        var userId = _userAccessor.GetUserId(Context);

        try
        {
            var mediaExists = await _mediaService.HasMediaAsync(iconId);

            if (!mediaExists)
                return HubResult<bool>.BadRequest(
                    "Invalid icon id.");

            await _profileService.SetNewIcon(
                userId,
                iconId);

            var payload = new AvatarUpdatePayload
            {
                UserId = userId,
                IconId = iconId
            };

            await NotifyProfileUpdatedAsync(
                userId,
                AvatarUpdatedEvent,
                payload);

            return HubResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update avatar for user {UserId}. IconId: {IconId}",
                userId,
                iconId);

            return HubResult<bool>.Error(
                "Server error.");
        }
    }

    private async Task NotifyProfileUpdatedAsync(
        Guid userId,
        string eventName,
        object payload)
    {
        try
        {
            var recipients = await GetProfileUpdateRecipientsAsync(userId);

            if (recipients.Count == 0)
                return;

            var groups = recipients
                .Select(GetUserGroup)
                .ToArray();

            await Clients
                .Groups(groups)
                .SendAsync(eventName, payload);

            _logger.LogDebug(
                "Profile update {EventName} for user {UserId} sent to {RecipientCount} users",
                eventName,
                userId,
                recipients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to notify profile update for user {UserId}",
                userId);
        }
    }

    private async Task<HashSet<Guid>> GetProfileUpdateRecipientsAsync(
        Guid userId)
    {
        var recipients = new HashSet<Guid>();

        // User himself.
        recipients.Add(userId);

        // Friends.
        try
        {
            var friends = await _friendshipService.GetFriendsAsync(userId);

            foreach (var friend in friends)
            {
                if (friend.Id != Guid.Empty)
                    recipients.Add(friend.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get friends for user {UserId}",
                userId);
        }

        // Potential friends.
        try
        {
            var potentialFriends =
                await _friendshipService.GetPotentialFriendsAsync(userId);

            foreach (var potentialFriend in potentialFriends)
            {
                if (potentialFriend.Id != Guid.Empty)
                    recipients.Add(potentialFriend.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get potential friends for user {UserId}",
                userId);
        }

        return recipients;
    }

    private static string GetUserGroup(Guid userId)
    {
        return $"{UserGroupPrefix}{userId}";
    }
}