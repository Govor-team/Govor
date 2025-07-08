using Govor.Core.Models;

namespace Govor.Application.Interfaces.Friends;

public interface IFriendshipService
{
    Task<List<User>> GetFriendsAsync(Guid userId);
    Task<List<User>> SearchUsersAsync(string query, Guid currentId);
}