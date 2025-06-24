using Govor.Core.Models;

namespace Govor.Core.Repositories.Invaites;

public interface IInvitesReader
{
    Task<List<Invitation>> GetAllAsync();
    Task<Invitation> GetByIdAsync(Guid id);
    Task<Invitation> GetByCodeAsync(string code);
    Task<List<Invitation>> GetAdminsInvitesAsync();
}