using Govor.Application.Exceptions.AuthService;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.Application.Interfaces.Authentication;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Admins;

namespace Govor.Application.Services.Authentication;

public class AuthService : IAccountService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUsersRepository _usersRepository;
    private readonly IAdminsRepository _adminsRepository;
    private readonly IUsernameValidator _usernameValidator;
    
    public AuthService(IUsersRepository usersRepository, 
        IPasswordHasher passwordHasher,
        IAdminsRepository adminsRepository,
        IUsernameValidator usernameValidator
       )
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _adminsRepository = adminsRepository;
        _usernameValidator = usernameValidator;
    }
    
    public async Task<User> RegistrationAsync(string name, string password, Invitation invitation)
    {
        _usernameValidator.Validate(name);
        
        if (await _usersRepository.ExistsUsernameAsync(name))
            throw new UserAlreadyExistException(name);
        
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
        
        await _usersRepository.AddAsync(user);
        
        await SetRole(user, invitation);
        
        return user;
    }


    public async Task<User> LoginAsync(string name, string password)
    {
        if (await _usersRepository.ExistsUsernameAsync(name) == false)
            throw new UserNotRegisteredException(name);
        
        var user = await _usersRepository.FindByUsernameAsync(name);
        
        if (_passwordHasher.Verify(password, user.PasswordHash) == false)
            throw new LoginUserException();
        
        return user;
    }
    
    /*
    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(refreshToken);
        
            var userId = Guid.Parse(principal?.FindFirst("userId")?.Value ?? string.Empty);

            var storedTokens = await _userSessionsRepository.GetUserTokensAsync(userId);
        
            if (!storedTokens.Contains(refreshToken))
                throw new UnauthorizedAccessException("Invalid refresh token");

            var user = await _usersRepository.FindByIdAsync(userId);
            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user);
            return newAccessToken;
        }
        catch (SecurityTokenException ex)
        {
            throw new InvalidOperationException("Invalid refresh token", ex);
        }
    }
    */
    
    private async Task SetRole(User user, Invitation invitation)
    {
        if(invitation.IsAdmin)
            await _adminsRepository.AddAsync(new Admin() { UserId = user.Id });
    }
}