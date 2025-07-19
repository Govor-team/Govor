using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces.UserSession;


public interface IUserSessionOpener
{
    Task<RefreshResult> OpenSessionAsync(User user, string deviceInfo);
}

