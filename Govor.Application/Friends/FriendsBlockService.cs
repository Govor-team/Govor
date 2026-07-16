namespace Govor.Application.Friends;

public class FriendsBlockService : IFriendsBlockService
{
    public Task BlockFriendRequestAsync(Guid userId, Guid currentUserId)
    {
        throw new NotImplementedException();
    }

    public Task UnblockFriendRequestAsync(Guid userId, Guid currentUserId)
    {
        throw new NotImplementedException();
    }
}