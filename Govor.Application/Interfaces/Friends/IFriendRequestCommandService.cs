using Govor.Core.Models;

namespace Govor.Application.Interfaces.Friends;

public interface IFriendRequestCommandService
{
    Task<Friendship> SendAsync(Guid fromUserId, Guid toUserId);
    Task<Friendship> AcceptAsync(Guid requestId, Guid currentUserId);
    Task<Friendship> RejectAsync(Guid requestId, Guid currentUserId);
}
