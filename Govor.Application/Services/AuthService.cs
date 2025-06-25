using Govor.API.Services.Authentication.Interfaces;
using Govor.Application.Exceptions.AuthService;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.Application.Interfaces.Authentication;
using Govor.Core.Repositories.Admins;

namespace Govor.Application.Services;

public class AuthService : IAccountService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUsersRepository _usersRepository;
    private readonly IAdminsRepository _adminsRepository;
    
    public AuthService(IUsersRepository usersRepository, 
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IAdminsRepository adminsRepository
       )
    {
        _usersRepository = usersRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _adminsRepository = adminsRepository;
    }
    
    public async Task<string> RegistrationAsync(string name, string password, Invitation invitation)
    {
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

    private async void SetRole(User user, Invitation invitation)
    {
        if(invitation.IsAdmin)
            await _adminsRepository.AddAsync(new Admin() { UserId = user.Id });
    }
}