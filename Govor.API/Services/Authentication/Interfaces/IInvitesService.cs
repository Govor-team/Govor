using Govor.Core.Models;

namespace Govor.API.Services.Authentication.Interfaces;

public interface IInvitesService
{
    public Task<string> GetRole(User user);
    public Invitation Validate(string inviteCode);
}