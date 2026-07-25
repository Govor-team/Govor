using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.Users.UserSessions;

public interface IUserSessionReader
{
    Task<Result<List<Domain.Models.Users.UserSession>, Error>> GetAllSessionsAsync(Guid userId);
}