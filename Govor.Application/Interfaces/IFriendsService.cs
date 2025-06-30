using Govor.Core.Models;

namespace Govor.Application.Interfaces;

public interface IFriendsService
{
    Task<List<User>> SearchUsersAsync(string query, Guid currentId);
    Task SendFriendRequestAsync(Guid fromUserId, Guid toUserId);
    Task AcceptFriendRequestAsync(Guid requestId, Guid currentUserId);
    Task<List<User>> GetFriendsAsync(Guid userId);
    Task<List<Friendship>> GetIncomingRequestsAsync(Guid userId);
}