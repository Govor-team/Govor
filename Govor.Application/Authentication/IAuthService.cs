using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;

namespace Govor.Application.Authentication;

public interface IAccountService
{
    public Task<Result<User>> RegistrationAsync(string name, string password, Invitation invitation);
    public Task<Result<User>> LoginAsync(string name, string password);
}