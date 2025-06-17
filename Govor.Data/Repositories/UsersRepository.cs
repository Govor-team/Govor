using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories;
using Govor.Data.Repositories.Exceptions;
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

    public async Task<User> FindById(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
               ?? throw new NotFoundByKeyException<Guid>(id, "User with given id does not exist");
    }

    public async Task<List<User>> FindByRangeId(IEnumerable<Guid> ids)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<IEnumerable<Guid>>(ids,"Users with given ids not found"));
    }
    
    public async Task<List<User>> FindUsersByName(string username)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Username == username)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<string>(username, "Users with given username not found"));
    }
    
    public async Task<List<User>> FindByRangeUsername(IEnumerable<string> usernames)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => usernames.Contains(x.Username))
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<IEnumerable<string>>(usernames, "Users with given usernames not found"));
    }

    public Task<List<User>> FindUsersByCreatedDate(DateOnly createdDate)
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