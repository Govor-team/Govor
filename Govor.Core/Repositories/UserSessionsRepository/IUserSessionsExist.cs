using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.UserSessionsRepository;

public interface IUserSessionsExist
{
    public bool Exist(Guid sessionId);
    public bool Exist(string hashedToken);
    public bool Exist(UserSession userSession);
}