using Govor.API.Services.Authentication.Interfaces;
using Govor.Core;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.API.Services;
using Govor.Core.Repositories.Admins;
using Govor.Core.Repositories.Invaites;


namespace Govor.API.Services.Authentication;

public class AuthService : IAccountService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUsersRepository _usersRepository;
    private readonly IInvitesRepository _invitesRepository;
    private readonly IAdminsRepository _adminsRepository;
    
    public AuthService(IUsersRepository usersRepository, 
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IInvitesRepository invitesRepository,
        IAdminsRepository adminsRepository)
    {
        _usersRepository = usersRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _invitesRepository = invitesRepository;
        _adminsRepository = adminsRepository;
    }
    
    public async Task<string> RegistrationAsync(string name, string password, string inviteCode)
    {
        // 1. Проверка существования имени
        if (await _usersRepository.ExistsUsernameAsync(name))
            throw new UserAlreadyExistException(name);

        // 2. Проверка валидности инвайта
        var invite = await _invitesRepository.FindByCodeAsync(inviteCode);

        // 3. Генерация пароля
        var passwordHash = _passwordHasher.Hash(password);

        // 4. Создание пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = name,
            PasswordHash = passwordHash,
            Description = string.Empty,
            CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            IconId = Guid.NewGuid(),
            WasOnline = DateTime.UtcNow,
            InviteId = invite.Id
        };

        // 5. Добавление пользователя
        await _usersRepository.AddAsync(user);

        // 6. Назначение роли, если инвайт — админский
        if (invite.IsAdmin)
        {
            await _adminsRepository.AddAsync(new Admin
            {
                UserId = user.Id
            });
        }

        // 7. Генерация токена
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