using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.Users.UserSessions;


public interface IUserSessionRevoker
{
    Task<Result<Unit, Error>> CloseSessionByIdAsync(Guid sessionId, Guid userId);
    Task<Result<Unit, Error>> CloseAllSessionsAsync(Guid userId);
}
