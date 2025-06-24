using Govor.Core.Models;

namespace Govor.Core.Repositories.Admins;

public interface IAdminsExist
{
    bool Exist(Guid guid);
    bool Exist(Admin admin);
}