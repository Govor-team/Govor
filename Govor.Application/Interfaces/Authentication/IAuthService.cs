using Govor.Core.Models;

namespace Govor.Application.Interfaces.Authentication;

public interface IAccountService
{
    public Task<string> RegistrationAsync(string name, string password, Invitation invitation);
    public Task<string> LoginAsync(string name, string password);
}