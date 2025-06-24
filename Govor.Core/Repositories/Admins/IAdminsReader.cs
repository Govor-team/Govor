using Govor.Core.Models;

namespace Govor.Core.Repositories.Admins;

public interface IAdminsReader
{
    Task<List<Admin>> GetAllAsync();
    Task<Admin> GetByIdAsync(Guid id);
}