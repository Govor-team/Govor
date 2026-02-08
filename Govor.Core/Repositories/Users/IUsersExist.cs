using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.Users;

public interface IUsersExist
{
    public Task<bool> ExistsAsync(User user);
    public Task<bool> ExistsByIdAsync(Guid id);
    public Task<bool> ExistsUsernameAsync(string username);
}