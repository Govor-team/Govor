using Govor.Core.Models;

namespace Govor.Core.Repositories.Admins;

public interface IAdminsWriter
{
    Task AddAsync(Admin admin);
    Task UpdateAsync(Admin admin);
    Task RemoveAsync(Guid admin);
}