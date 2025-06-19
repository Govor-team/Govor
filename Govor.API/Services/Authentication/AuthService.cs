using Govor.Core;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories;
using Govor.Core.Repositories.Users;
using Govor.Core.Services;
using Govor.Data.Repositories;

namespace Govor.API.Services.Authentication;

public class AuthService : IAccountService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUsersRepository _usersRepository;
    
    public AuthService(IUsersRepository usersRepository, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _usersRepository = usersRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<string> RegistrationAsync(string name, string password)
    {
        if (await _usersRepository.ExistsUsernameAsync(name))
            throw new UserAlreadyExistException(name);
        
        var passwordHash = _passwordHasher.Hash(password);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = name,
            Description = string.Empty,
            PasswordHash = passwordHash,
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
            IconId = Guid.NewGuid(),
            WasOnline = DateTime.UtcNow
            //Role = role == "Admin" ? "Admin" : "User" // Ограничение ролей
        };
        
        await _usersRepository.AddAsync(user);
        
        return _jwtService.GenerateJwtToken(user);
    }

    public async Task<string> LoginAsync(string name, string password)
    {
        if (await _usersRepository.ExistsUsernameAsync(name) == false)
            throw new UserNotRegisteredException(name);
        
        var user = await _usersRepository.FindByUsernameAsync(name);
        
        if (_passwordHasher.Verify(password, user.PasswordHash) == false)
            throw new LoginUserException();
        
        return _jwtService.GenerateJwtToken(user);
    }
}

public class LoginUserException : GovorCoreException { }

public class UserAlreadyExistException(string username) : GovorCoreException($"{username} is already exists!") { }

public class UserNotRegisteredException(string username) : GovorCoreException($"{username} is not registered!") { }