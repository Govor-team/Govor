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
            .ToListOrThrowIfEmpty(new NotFoundException("Users in Database not exists"));
    }

    public async Task<User> FindById(Guid id)
    {
        if(id == Guid.Empty)
            throw new ArgumentException("Id must not be empty", nameof(id));
        
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
               ?? throw new NotFoundByKeyException<Guid>(id, "User with given id does not exist");
    }

    public async Task<List<User>> FindByRangeId(IEnumerable<Guid> ids)
    {
        if (ids is null || !ids.Any())
            throw new ArgumentException("Ids must not be empty", nameof(ids));
        
        return await _context.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<IEnumerable<Guid>>(ids,"Users with given ids not found"));
    }
    
    public async Task<User> FindByUsername(string username)
    {
        if(username is null || username == string.Empty)
            throw new ArgumentNullException(username, "Username cannot be empty");
        
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username) 
               ?? throw new NotFoundByKeyException<string>(username, "User with given username does not exist");
    }
    
    public async Task<List<User>> FindByRangeUsernames(IEnumerable<string> usernames)
    {
        if (usernames is null || !usernames.Any())
            throw new ArgumentException("Usernames must not be empty", nameof(usernames));

        return await _context.Users
            .AsNoTracking()
            .Where(x => usernames.Contains(x.Username))
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<IEnumerable<string>>(usernames, "Users with given usernames not found"));
    }

    public async Task<List<User>> FindUsersByCreatedDate(DateOnly createdDate)
    {
        if(createdDate == DateOnly.MinValue)
            throw new ArgumentException("Created date cannot be earlier than MinValue", nameof(createdDate));

        return await _context.Users
            .AsNoTracking()
            .Where(u => createdDate == u.CreatedOn)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<DateOnly>(createdDate, "Users with given created date do not exist"));
    }
    
    public async Task Add(User user)
    {
        try
        {
            _validator.Validate(user);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<User> ex)
        {
            throw new AdditionUserException("User with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionUserException("Cannot add user", ex);
        }
    }

    public async Task Update(User user)
    {
        try
        {
            _validator.Validate(user);

            var rowsAffected = await _context.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(a => a.Username, user.Username)
                    .SetProperty(u => u.IconId, user.IconId)
                    .SetProperty(u => u.Description, user.Description)
                    .SetProperty(u => u.CreatedOn, user.CreatedOn)
                    .SetProperty(u => u.HashPassword, user.HashPassword)
                    .SetProperty(u => u.WasOnline, user.WasOnline)
                );

            if (rowsAffected == 0)
                throw new NotFoundByKeyException<Guid>(user.Id);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new UserUpdateException($"Not found user by given id {user.Id}", ex);
        }
        catch (Exception ex)
        {
            throw new UserUpdateException($"Error when updating the user {user.Id}", ex);
        }
    }

    public Task Remove(User user)
    {
        _validator.Validate(user);
        throw new NotImplementedException();
    }

    public Task Remove(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Exists(User user)
    {
        _validator.Validate(user);
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

public class AdditionUserException(string s, Exception ex) 
    : Exception(s, ex);

public class UserUpdateException(string s, Exception ex)
    : Exception(s, ex);
    