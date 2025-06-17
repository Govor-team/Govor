using Govor.Core.Models;

namespace Govor.Core.Repositories;

public interface IUsersWriter
{
    public Task AddAsync(User user);
    public Task UpdateAsync(User user);
    public Task RemoveAsync(User user);
    public Task RemoveAsync(Guid userId);
}