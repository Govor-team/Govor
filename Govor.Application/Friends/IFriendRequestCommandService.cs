using Govor.Domain.Common;
using Govor.Domain.Models;

namespace Govor.Application.Friends;

public interface IFriendRequestCommandService
{
    Task<Result<Friendship>> SendAsync(Guid fromUserId, Guid toUserId);
    Task<Result<Friendship>> AcceptAsync(Guid requestId, Guid currentUserId);
    Task<Result<Friendship>> RejectAsync(Guid requestId, Guid currentUserId);
}
