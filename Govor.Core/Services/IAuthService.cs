namespace Govor.Core.Services;

public interface IAccountService
{
    public Task RegistrationAsync(string name, string password);
    public Task<string> LoginAsync(string name, string password);
}