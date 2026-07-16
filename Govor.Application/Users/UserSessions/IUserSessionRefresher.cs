using Govor.Domain.Common;

namespace Govor.Application.Users.UserSessions;

public interface IUserSessionRefresher
{
     Task<Result<RefreshResult>> RefreshTokenAsync(string refreshToken);
}