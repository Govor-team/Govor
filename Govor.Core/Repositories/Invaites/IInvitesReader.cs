using Govor.Core.Models;

namespace Govor.Core.Repositories.Invaites;

public interface IInvitesReader
{
    Task<List<Invitation>> GetAllAsync();
    Task<Invitation> FindByIdAsync(Guid id);
    Task<Invitation> FindByCodeAsync(string code);
    Task<List<Invitation>> FindAdminsInvitesAsync();
}