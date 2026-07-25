using Govor.Domain.Common;
using Govor.Domain.Models;
using SmartRes;

namespace Govor.Application.Infrastructure.AdminsStuff;

public interface IInvitationGetter
{
    Task<List<Invitation>> GetAllAsync();
    Task<Result<Invitation, Error>> FindByIdAsync(Guid id);
}