using Govor.Core.Models;

namespace Govor.Application.Interfaces.Friends;

public interface IFriendRequestService
{
    Task SendFriendRequestAsync(Guid fromUserId, Guid toUserId);
    Task AcceptFriendRequestAsync(Guid requestId, Guid currentUserId);
    Task RejectFriendRequestAsync(Guid requestId, Guid currentUserId);
    Task<List<Friendship>> GetIncomingRequestsAsync(Guid userId);
    Task<List<Friendship>> GetResponsesAsync(Guid userId);
}