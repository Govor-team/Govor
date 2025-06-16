using Govor.Core.Models;

namespace Govor.Core.Repositories;

public interface IUsersExist
{
    public Task<bool> Exists(User user);
    public Task<bool> ExistsById(Guid id);
    public Task<bool> ExistsUsername(string username);
}