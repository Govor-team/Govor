using Govor.Core.Models;

namespace Govor.Core.Repositories.Friendships;

public interface IFriendshipsReader
{
    Task<List<Friendship>> GetAllAsync();
    Task<Friendship> GetByIdAsync(Guid id);
    Task<List<Friendship>> FindByUserIdAsync(Guid userId);
}