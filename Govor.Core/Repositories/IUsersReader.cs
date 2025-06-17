using Govor.Core.Models;

namespace Govor.Core.Repositories;

public interface IUsersReader
{
    public Task<IEnumerable<User>> GetAll();
    public Task<User> FindById(Guid id);
    public Task<List<User>> FindByRangeId(IEnumerable<Guid> ids);
    public Task<List<User>> FindUsersByName(string username);
    public Task<List<User>> FindByRangeUsername(IEnumerable<string> usernames);
    public Task<List<User>> FindUsersByCreatedDate(DateOnly createdDate);
}