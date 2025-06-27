using Govor.Core.Models;

namespace Govor.Core.Repositories.Friendships;

public interface IFriendshipsWriter
{
    Task AddAsync(Friendship friendship);
    Task UpdateAsync(Friendship friendship);
    Task RemoveAsync(Friendship friendship);
}