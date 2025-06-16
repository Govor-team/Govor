using Govor.Core.Models;

namespace Govor.Core.Repositories;

public interface IUsersWriter
{
    public Task Add(User user);
    public Task Update(User user);
    public Task Remove(User user);
    public Task Remove(Guid userId);
}