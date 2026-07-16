using Govor.Domain.Common;
using Govor.Domain.Models;

namespace Govor.Application.Infrastructure.AdminsStuff;

public interface IInvitationGetter
{
    Task<List<Invitation>> GetAllAsync();
    Task<Result<Invitation>> FindByIdAsync(Guid id);
}