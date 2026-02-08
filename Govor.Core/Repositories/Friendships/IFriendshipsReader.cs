using Govor.Core.Models;

namespace Govor.Core.Repositories.Friendships;

public interface IFriendshipsReader
{
    Task<List<Friendship>> GetAllAsync();
    Task<Friendship> GetByIdAsync(Guid id);
    Task<Friendship> GetFriendshipAsync(Guid fromUserId, Guid toUserId);
    Task<List<Friendship>> FindByUserIdAsync(Guid userId);
}