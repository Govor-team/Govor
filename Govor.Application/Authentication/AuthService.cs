using Govor.Application.Authentication.Exceptions;
using Govor.Application.Infrastructure.Validators;
using Govor.Application.Users;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using SmartRes;

namespace Govor.Application.Authentication;

public class AuthService : IAccountService
{
    private readonly GovorDbContext _context; 
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserNameExistValidator _userNameExistValidator;
    private readonly IUsernameValidator _usernameValidator;
    
    public AuthService(
        GovorDbContext context,
        IUserNameExistValidator existValidator,
        IPasswordHasher passwordHasher,
        IUsernameValidator usernameValidator)
    {
        _context = context;
        _userNameExistValidator = existValidator;
        _passwordHasher = passwordHasher;
        _usernameValidator = usernameValidator;
    }
    
    public async Task<Result<User, Error>> RegistrationAsync(string name, string password, Invitation invitation)
    {
      
        var validationResult = _usernameValidator.Validate(name);
        if (validationResult.IsFailure)
        {
            return Result.Failure<User>(validationResult.Error);
        }
        
        if (await _userNameExistValidator.IsUsernameExistsAsync(name))
        {
            return Result.Failure<User>(Error.Conflict(
                 nameof(UserAlreadyExistException), 
                $"User with username '{name}' already exists."));
        }
        
        var passwordHash = _passwordHasher.Hash(password);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = name,
            PasswordHash = passwordHash,
            Description = string.Empty,
            CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            IconId = Guid.Empty,
            WasOnline = DateTime.UtcNow,
            InviteId = invitation.Id
        };
        
       
        await _context.Users.AddAsync(user);
        
        await SetRoleAsync(user, invitation);
        
        // TODO: inv.participantCount -= 1; db.save();
        
        await _context.SaveChangesAsync();
        
        return user; // Success 
    }

    public async Task<Result<User, Error>> LoginAsync(string name, string password)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == name);
        
        if (user is null)
        {
            return Result.Failure<User>(Error.NotFound(
                nameof(UserNotRegisteredException), 
                $"User '{name}' is not registered."));
        }
        
        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result.Failure<User>(Error.Failure(
                nameof(InvalidOperationException), 
                "The password provided is incorrect."));
        }
        
        return user; // Success 
    }
    
    private async Task SetRoleAsync(User user, Invitation invitation)
    {
        if (invitation.IsAdmin)
        {
            await _context.Admins.AddAsync(new Admin { UserId = user.Id });
        }
    }
}
