using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces;

public interface IUsersAdministration
{
    Task<List<User>> GetAllUsersAsync();
    Task<User> GetUserById(Guid userId);
}