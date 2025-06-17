using Govor.Core.Models;

namespace Govor.Core.Repositories;

public interface IUsersExist
{
    public Task<bool> ExistsAsync(User user);
    public Task<bool> ExistsByIdAsync(Guid id);
    public Task<bool> ExistsUsernameAsync(string username);
}