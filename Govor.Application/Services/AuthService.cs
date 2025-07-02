using System.Text.RegularExpressions;
using Govor.API.Services.Authentication.Interfaces;
using Govor.Application.Exceptions.AuthService;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.Application.Interfaces.Authentication;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Repositories.Admins;

namespace Govor.Application.Services;

public class AuthService : IAccountService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUsersRepository _usersRepository;
    private readonly IAdminsRepository _adminsRepository;
    private readonly IUsernameValidator _usernameValidator;
    
    public AuthService(IUsersRepository usersRepository, 
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IAdminsRepository adminsRepository,
        IUsernameValidator usernameValidator
       )
    {
        _usersRepository = usersRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _adminsRepository = adminsRepository;
        _usernameValidator = usernameValidator;
    }
    
    public async Task<string> RegistrationAsync(string name, string password, Invitation invitation)
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
            IconId = Guid.NewGuid(),
            WasOnline = DateTime.UtcNow,
            InviteId = invitation.Id
        };
        
        await _usersRepository.AddAsync(user);
        
        SetRole(user, invitation);
        
        return await _jwtService.GenerateJwtTokenAsync(user);
    }


    public async Task<string> LoginAsync(string name, string password)
    {
        if (await _usersRepository.ExistsUsernameAsync(name) == false)
            throw new UserNotRegisteredException(name);
        
        var user = await _usersRepository.FindByUsernameAsync(name);
        
        if (_passwordHasher.Verify(password, user.PasswordHash) == false)
            throw new LoginUserException();
        
        return await _jwtService.GenerateJwtTokenAsync(user);
    }

    private async void SetRole(User user, Invitation invitation)
    {
        if(invitation.IsAdmin)
            await _adminsRepository.AddAsync(new Admin() { UserId = user.Id });
    }
}