using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class UsersRepository : IUsersRepository
{
    private GovorDbContext _context;
    private IObjectValidator<User> _validator;
    public UsersRepository(GovorDbContext context, IObjectValidator<User> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<IEnumerable<User>> GetAll()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => true)
            .ToListAsync();
    }

    public Task<User> FindById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> FindByRangeId(IEnumerable<Guid> ids)
    {
        throw new NotImplementedException();
    }

    public Task<User> FindByUsername(string username)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> FindByRangeUsername(IEnumerable<string> usernames)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> FindUsersByCreatedDate(DateOnly createdDate)
    {
        throw new NotImplementedException();
    }

    public Task Add(User user)
    {
        throw new NotImplementedException();
    }

    public Task Update(User user)
    {
        throw new NotImplementedException();
    }

    public Task Remove(User user)
    {
        throw new NotImplementedException();
    }

    public Task Remove(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Exists(User user)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsUsername(string username)
    {
        throw new NotImplementedException();
    }
}