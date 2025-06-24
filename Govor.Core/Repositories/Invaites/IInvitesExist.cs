using Govor.Core.Models;

namespace Govor.Core.Repositories.Invaites;

public interface IInvitesExist
{
    bool Exist(Invitation invitation);
    bool Exist(Guid guid);
}