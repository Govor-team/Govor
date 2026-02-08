using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.Admins;

public interface IAdminsReader
{
    Task<List<Admin>> GetAllAsync();
    Task<Admin> GetByIdAsync(Guid id);
}