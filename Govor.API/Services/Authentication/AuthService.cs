using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Repositories;
using Govor.Core.Services;

namespace Govor.API.Services.Authentication;

public class AuthService : IAccountService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUsersRepository _usersRepository;

    public AuthService(IUsersRepository usersRepository, IPasswordHasher passwordHasher)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
    }
    public Task RegistrationAsync(string name, string password)
    {
        throw new NotImplementedException();
    }

    public async Task<string> LoginAsync(string name, string password)
    {
        if (await _usersRepository.ExistsUsernameAsync(name) == false)
            throw new UserNotRegisteredException(name);

        throw new NotImplementedException();
    }
}

public class UserAlreadyExistException(string username) : Exception($"{username} is already exists!") { }

public class UserNotRegisteredException(string username) : Exception($"{username} is not registered!") { }