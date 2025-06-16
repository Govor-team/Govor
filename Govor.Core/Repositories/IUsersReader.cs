using Govor.Core.Models;

namespace Govor.Core.Repositories;

public interface IUsersReader
{
    public Task<IEnumerable<User>> GetAll();
    public Task<User> FindById(Guid id);
    public Task<IEnumerable<User>> FindByRangeId(IEnumerable<Guid> ids);
    public Task<User> FindByUsername(string username);
    public Task<IEnumerable<User>> FindByRangeUsername(IEnumerable<string> usernames);
    
    public Task<IEnumerable<User>> FindUsersByCreatedDate(DateOnly createdDate);
}