using Govor.Application.Interfaces;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Infrastructure.AdminsStuff;

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

    public async Task<User> GetUserById(Guid userId)
    {
        var result = await _usersRepository.FindByIdAsync(userId);
            
        return result;
    }
}