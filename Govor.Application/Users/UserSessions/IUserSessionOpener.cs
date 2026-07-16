using Govor.Domain.Common;
using Govor.Domain.Models.Users;

namespace Govor.Application.Users.UserSessions;


public interface IUserSessionOpener
{
    Task<Result<RefreshResult>> OpenSessionAsync(User user, string deviceInfo);
}

