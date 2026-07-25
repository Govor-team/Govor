using Govor.Domain.Common;
using Govor.Domain.Models;
using SmartRes;

namespace Govor.Application.Friends;

public interface IFriendRequestCommandService
{
    Task<Result<Friendship, Error>> SendAsync(Guid fromUserId, Guid toUserId);
    Task<Result<Friendship, Error>> AcceptAsync(Guid requestId, Guid currentUserId);
    Task<Result<Friendship, Error>> RejectAsync(Guid requestId, Guid currentUserId);
}
