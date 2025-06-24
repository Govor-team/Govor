using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.Admins;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class AdminsRepository(GovorDbContext context) : IAdminsRepository
{
    private GovorDbContext _context = context;

    public async Task<List<Admin>> GetAllAsync()
    {
        return await _context.Admins
            .AsNoTracking()
            .Include(a => a.User)
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public Task<Admin> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Admin admin)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Admin admin)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid admin)
    {
        throw new NotImplementedException();
    }

    public bool Exist(Guid guid)
    {
        throw new NotImplementedException();
    }

    public bool Exist(Admin admin)
    {
        throw new NotImplementedException();
    }
}