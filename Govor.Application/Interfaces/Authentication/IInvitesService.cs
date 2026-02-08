using Govor.Core.Models;
using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces.Authentication;

public interface IInvitesService
{
    public Task<string> GetRoleAsync(User user);
    public Task<Invitation> ValidateAsync(string inviteCode);
}