using Govor.Core.Models;

namespace Govor.Core.Services;

public interface IAccountService
{
    public Task<string> RegistrationAsync(string name, string password);
    public Task<string> LoginAsync(string name, string password);
}