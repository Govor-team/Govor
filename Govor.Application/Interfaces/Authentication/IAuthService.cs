using Govor.Core.Models;
using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces.Authentication;

public interface IAccountService
{
    public Task<User> RegistrationAsync(string name, string password, Invitation invitation);
    public Task<User> LoginAsync(string name, string password);
}