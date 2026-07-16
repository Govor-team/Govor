using Govor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Users;

public class UserNameExistValidator : IUserNameExistValidator
{
    private readonly GovorDbContext _context;
    
    public UserNameExistValidator(GovorDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsUsernameExistsAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }
        
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == userName);
    }
}