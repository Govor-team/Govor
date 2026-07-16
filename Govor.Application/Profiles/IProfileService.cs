using Govor.Domain.Common;

namespace Govor.Application.Profiles;

public interface IProfileService
{
    public Task<Result<UserProfile>> GetUserProfileAsync(Guid userId);
    public Task<Result> SetDescription(string description, Guid userId);
    public Task<Result> SetNewIcon(Guid userId, Guid iconId);
}
