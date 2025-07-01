using Govor.Core.Models;

namespace Govor.API.Services.Authentication.Interfaces;

public interface IInvitesService
{
    public Task<string> GetRoleAsync(User user);
    public Task<Invitation> ValidateAsync(string inviteCode);
}