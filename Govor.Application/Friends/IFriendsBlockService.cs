namespace Govor.Application.Friends;

public interface IFriendsBlockService
{
    Task BlockFriendRequestAsync(Guid userId, Guid currentUserId);
    Task UnblockFriendRequestAsync(Guid userId, Guid currentUserId);
}