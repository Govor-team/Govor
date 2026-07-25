using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.Users.UserSessions;

public interface IUserSessionRefresher
{
     Task<Result<RefreshResult, Error>> RefreshTokenAsync(string refreshToken);
}