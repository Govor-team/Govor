using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.Profiles;

public interface IProfileService
{
    public Task<Result<UserProfile, Error>> GetUserProfileAsync(Guid userId);
    public Task<Result<Unit, Error>> SetDescription(string description, Guid userId);
    public Task<Result<Unit, Error>> SetNewIcon(Guid userId, Guid iconId);
}
