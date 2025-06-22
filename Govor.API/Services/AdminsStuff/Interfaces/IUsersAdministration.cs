using Govor.Core.Models;

namespace Govor.API.Services.AdminsStuff.Interfaces;

public interface IUsersAdministration
{
    Task<List<User>> GetAllUsersAsync();
}