using Govor.Domain.Models;
using Govor.Domain.Models.Users;

namespace Govor.Application.Infrastructure.Extensions;

public interface IFriendsService
{
    Task<List<User>> SearchUsersAsync(string query, Guid currentId);
    Task SendFriendRequestAsync(Guid fromUserId, Guid toUserId);
    Task AcceptFriendRequestAsync(Guid requestId, Guid currentUserId);
    Task RejectFriendRequestAsync(Guid requestId, Guid currentUserId);
    Task<List<User>> GetFriendsAsync(Guid userId);
    Task<List<Friendship>> GetResponsesAsync(Guid userId);
    Task<List<Friendship>> GetIncomingRequestsAsync(Guid userId);
}


