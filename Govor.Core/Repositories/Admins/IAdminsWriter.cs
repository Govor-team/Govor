using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.Admins;

public interface IAdminsWriter
{
    Task AddAsync(Admin admin);
    Task UpdateAsync(Admin admin);
    Task RemoveAsync(Guid admin);
}