using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.UserOnlineStatus;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.UserOnlineStatus;

public class UserNotificationScopeService : IUserNotificationScopeService
{
    private readonly ILogger<UserNotificationScopeService> _logger;
    private readonly IFriendshipService _friendships;

    public UserNotificationScopeService(ILogger<UserNotificationScopeService> logger, IFriendshipService friendships)
    {
        _logger = logger;
        _friendships = friendships;
    }

    public async Task<List<Guid>> GetNotifiedUsers(Guid userId)
    {
        try
        {
            _logger.LogInformation($"Getting notified users of online/offline action user {userId}");
            var users = await _friendships.GetFriendsAsync(userId);
            return users.Select(u => u.Id).ToList();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning(ex, ex.Message);
            throw new InvalidOperationException("User not found");
        }
    }

    /*public async Task SetWasOnlineAsync(Guid userId, DateTime when)
    {
        try
        {
            _logger.LogInformation("Set was-online for user {UserId}", userId);
            var user = await _users.FindByIdAsync(userId);
            user.WasOnline = when;
            await _users.UpdateAsync(user);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new InvalidOperationException("User not found");
        }
        catch (UpdateException ex)
        {
            _logger.LogError(ex, "Failed to set WasOnline for user {UserId} at {Time}", userId, when);
            throw new InvalidOperationException("Something went wrong when trying to set was-online for user");
        }
    }*/
}