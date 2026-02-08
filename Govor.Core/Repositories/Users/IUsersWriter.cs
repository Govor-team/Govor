using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.Users;

public interface IUsersWriter
{
    public Task AddAsync(User user);
    public Task UpdateAsync(User user);
    public Task RemoveAsync(User user);
    public Task RemoveAsync(Guid userId);
}