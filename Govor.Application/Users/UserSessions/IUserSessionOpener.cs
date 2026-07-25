using Govor.Domain.Common;
using Govor.Domain.Models.Users;
using SmartRes;

namespace Govor.Application.Users.UserSessions;


public interface IUserSessionOpener
{
    Task<Result<RefreshResult, Error>> OpenSessionAsync(User user, string deviceInfo);
}

