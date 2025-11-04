using Govor.Application.Profiles;

namespace Govor.Application.Interfaces;

public interface IProfileService
{
    public Task<UserProfile> GetUserProfileAsync(Guid userId);
    public Task SetDescription(string description, Guid userId);
    public Task SetNewIcon(Guid userId, Guid iconId);
}
