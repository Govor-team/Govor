using Govor.Domain.Models.Users;

namespace Govor.Application.Infrastructure.AdminsStuff;

public interface IUsersAdministration
{
    Task<List<User>> GetAllUsersAsync();
    Task<User> GetUserById(Guid userId);
    Task SetPasswordAsync(Guid userId, string password);
}