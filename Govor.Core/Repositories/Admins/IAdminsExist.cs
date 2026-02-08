using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.Admins;

public interface IAdminsExist
{
    bool Exist(Guid guid);
    bool Exist(Admin admin);
}