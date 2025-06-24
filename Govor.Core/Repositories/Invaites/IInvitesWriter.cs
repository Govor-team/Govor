using Govor.Core.Models;

namespace Govor.Core.Repositories.Invaites;

public interface IInvitesWriter
{
    Task AddAsync(Invitation invitation);
    Task UpdateAsync(Invitation invitation);
    Task RemoveAsync(Invitation invitation);
}