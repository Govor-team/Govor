using Govor.API.Services.AdminsStuff.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;

namespace Govor.API.Services.AdminsStuff;

public class UsersService : IUsersAdministration
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            var results = await _usersRepository.GetAllAsync();
            return results;
        }
        catch (NotFoundException ex)
        {
            return new List<User>();
        }
    }
}