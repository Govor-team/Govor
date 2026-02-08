using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces.Friends;

public interface IFriendshipService
{
    Task<List<User>> GetFriendsAsync(Guid userId);
    Task<List<User>> GetPotentialFriendsAsync(Guid userId);
    Task<List<User>> SearchUsersAsync(string query, Guid currentId);
}