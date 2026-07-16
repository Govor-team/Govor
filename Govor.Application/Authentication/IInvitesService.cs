using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;

namespace Govor.Application.Authentication;

public interface IInvitesService
{
    public Task<string> GetRoleNameAsync(User user);
    public Task<string> GetRoleNameAsync(Guid sessionId);
    public Task<Result<Invitation>> ValidateAsync(string inviteCode);
}