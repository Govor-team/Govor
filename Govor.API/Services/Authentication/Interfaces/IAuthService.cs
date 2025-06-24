using Govor.Core.Models;

namespace Govor.API.Services.Authentication.Interfaces;

public interface IAccountService
{
    public Task<string> RegistrationAsync(string name, string password, string inviteCode);
    public Task<string> LoginAsync(string name, string password);
}