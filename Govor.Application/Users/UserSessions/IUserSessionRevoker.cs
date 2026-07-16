using Govor.Domain.Common;

namespace Govor.Application.Users.UserSessions;


public interface IUserSessionRevoker
{
    Task<Result> CloseSessionByIdAsync(Guid sessionId, Guid userId);
    Task<Result> CloseAllSessionsAsync(Guid userId);
}
