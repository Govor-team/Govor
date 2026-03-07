using Govor.Application.Interfaces;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Infrastructure.AdminsStuff;

public class UsersService : IUsersAdministration
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    
    public UsersService(IUsersRepository usersRepository, IPasswordHasher passwordHasher)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
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

    public async Task SetPasswordAsync(Guid userId, string password)
    {
        try
        {
            var user = await _usersRepository.FindByIdAsync(userId);
            
            user.PasswordHash = _passwordHasher.Hash(password);
            
            await _usersRepository.UpdateAsync(user);
        }
        catch (NotFoundException ex)
        {
            throw new NotFoundException(ex.Message);
        }
    }
    
    public async Task<User> GetUserById(Guid userId)
    {
        var result = await _usersRepository.FindByIdAsync(userId);
            
        return result;
    }
}