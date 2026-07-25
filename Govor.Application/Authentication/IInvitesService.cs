using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;
using SmartRes;

namespace Govor.Application.Authentication;

public interface IInvitesService
{
    public Task<string> GetRoleNameAsync(User user);
    public Task<string> GetRoleNameAsync(Guid sessionId);
    public Task<Result<Invitation, Error>> ValidateAsync(string inviteCode);
}