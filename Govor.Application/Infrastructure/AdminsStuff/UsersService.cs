using Govor.Application.Authentication;
using Govor.Domain;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Infrastructure.AdminsStuff;

public class UsersService : IUsersAdministration
{
    private readonly GovorDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    
    public UsersService(GovorDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var results = await _context.Users
            .AsNoTracking()
            .Take(50)
            .ToListAsync();
        return results;
    }

    public async Task SetPasswordAsync(Guid userId, string password)
    {
        var user = await GetUserById(userId);

        if (user is null)
            return;

        user.PasswordHash = _passwordHasher.Hash(password);

        await _context.SaveChangesAsync();
    }
    
    public async Task<User> GetUserById(Guid userId)
    {
        var result = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
            
        return result;
    }
}