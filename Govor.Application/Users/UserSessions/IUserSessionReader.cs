using Govor.Domain.Common;

namespace Govor.Application.Users.UserSessions;

public interface IUserSessionReader
{
    Task<Result<List<Domain.Models.Users.UserSession>>> GetAllSessionsAsync(Guid userId);
}