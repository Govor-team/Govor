using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Admins;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class AdminsRepository(GovorDbContext context, IObjectValidator<Admin> validator) : IAdminsRepository
{
    private GovorDbContext _context = context;
    private IObjectValidator<Admin> _validator = validator;
    public async Task<List<Admin>> GetAllAsync()
    {
        return await _context.Admins
            .AsNoTracking()
            .Include(a => a.User)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public async Task<Admin> GetByIdAsync(Guid id)
    {
        return await _context.Admins
            .AsNoTracking()
            .Include(a => a.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.UserId == id)
            ?? throw new NotFoundByKeyException<Guid>(id, "User with given id does not exist");
    }

    public async Task AddAsync(Admin admin)
    {
        try
        {
            _validator.Validate(admin);

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<Admin> ex)
        {
            throw new AdditionException("Admin with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add admin", ex);
        }
    }

    public async Task UpdateAsync(Admin admin)
    {
        try
        {
            _validator.Validate(admin);

            var rowsAffected = await _context.Admins
                .Where(a => a.UserId == admin.UserId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(a => a.UserId, admin.UserId)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found admin by given id {admin.UserId}");

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the admin {admin.UserId}", ex);
        }
    }

    public async Task RemoveAsync(Guid admin)
    {
        try
        {
            var result = await GetByIdAsync(admin);

            _context.Admins.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found admin by given id {admin}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the admin", ex);
        }
    }

    public bool Exist(Guid id)
    {
        return _context.Users.Any(u => u.Id == id);
    }

    public bool Exist(Admin admin)
    {
        _validator.Validate(admin);
        
        return _context.Admins.Any(u =>
            u.UserId == admin.UserId
        );
    }
}