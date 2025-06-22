using Govor.Core.Models;

namespace Govor.Core.Repositories.Users;

public interface IUsersReader
{
    public Task<List<User>> GetAllAsync();
    public Task<User> FindByIdAsync(Guid id);
    public Task<List<User>> FindByRangeIdAsync(IEnumerable<Guid> ids);
    public Task<User> FindByUsernameAsync(string username);
    public Task<List<User>> FindByRangeUsernamesAsync(IEnumerable<string> usernames);
    public Task<List<User>> FindUsersByCreatedDateAsync(DateOnly createdDate);
}