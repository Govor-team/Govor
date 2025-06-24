using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories;
using Govor.Core.Repositories.Users;
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
    
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListOrThrowIfEmpty(new NotFoundException("Users in Database not exists"));
    }

    public async Task<User> FindByIdAsync(Guid id)
    {
        if(id == Guid.Empty)
            throw new ArgumentException("Id must not be empty", nameof(id));
        
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Invite)
            .FirstOrDefaultAsync(x => x.Id == id)
               ?? throw new NotFoundByKeyException<Guid>(id, "User with given id does not exist");
    }

    public async Task<List<User>> FindByRangeIdAsync(IEnumerable<Guid> ids)
    {
        if (ids is null || !ids.Any())
            throw new ArgumentException("Ids must not be empty", nameof(ids));
        
        return await _context.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Include(u => u.Invite)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<IEnumerable<Guid>>(ids,"Users with given ids not found"));
    }
    
    public async Task<User> FindByUsernameAsync(string username)
    {
        if(username is null || username == string.Empty)
            throw new ArgumentNullException(username, "Username cannot be empty");
        
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Invite)
            .FirstOrDefaultAsync(x => x.Username == username)
               ?? throw new NotFoundByKeyException<string>(username, "User with given username does not exist");
    }
    
    public async Task<List<User>> FindByRangeUsernamesAsync(IEnumerable<string> usernames)
    {
        if (usernames is null || !usernames.Any())
            throw new ArgumentException("Usernames must not be empty", nameof(usernames));

        return await _context.Users
            .AsNoTracking()
            .Where(x => usernames.Contains(x.Username))
            .Include(u => u.Invite)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<IEnumerable<string>>(usernames, "Users with given usernames not found"));
    }

    public async Task<List<User>> FindUsersByCreatedDateAsync(DateOnly createdDate)
    {
        if(createdDate == DateOnly.MinValue)
            throw new ArgumentException("Created date cannot be earlier than MinValue", nameof(createdDate));

        return await _context.Users
            .AsNoTracking()
            .Where(u => createdDate == u.CreatedOn)
            .Include(u => u.Invite)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<DateOnly>(createdDate, "Users with given created date do not exist"));
    }
    
    public async Task AddAsync(User user)
    {
        try
        {
            _validator.Validate(user);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<User> ex)
        {
            throw new AdditionException("User with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add user", ex);
        }
    }
    
    // TODO: Test this block
    public async Task UpdateAsync(User user)
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
                    .SetProperty(u => u.PasswordHash, user.PasswordHash)
                    .SetProperty(u => u.WasOnline, user.WasOnline)
                );

            if (rowsAffected == 0)
                throw new NotFoundByKeyException<Guid>(user.Id);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new UpdateException($"Not found user by given id {user.Id}", ex);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the user {user.Id}", ex);
        }
    }

    public async Task RemoveAsync(User user)
    {
        try
        {
            _validator.Validate(user);
            await RemoveAsync(user.Id);
        }
        catch (InvalidObjectException<User> ex)
        {
            throw new RemoveException("User with given data invalid", ex);
        }
    }

    public async Task RemoveAsync(Guid userId)
    {
        try
        {
            var result = await FindByIdAsync(userId);

            _context.Users.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found user by given id {userId}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the user", ex);
        }
    }

    public Task<bool> ExistsAsync(User user)
    {
        _validator.Validate(user);

        return _context.Users.AnyAsync(u =>
            u.Id == user.Id &&
            u.Username == user.Username &&
            u.PasswordHash == user.PasswordHash
        );
    }

    public Task<bool> ExistsByIdAsync(Guid id)
    {
        return _context.Users.AnyAsync(u => u.Id == id);
    }

    public Task<bool> ExistsUsernameAsync(string username)
    {
        return _context.Users.AnyAsync(u => u.Username == username);
    }

}