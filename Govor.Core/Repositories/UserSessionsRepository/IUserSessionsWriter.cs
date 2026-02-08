using Govor.Core.Models;
using Govor.Core.Models.Users;

namespace Govor.Core.Repositories.UserSessionsRepository;

public interface IUserSessionsWriter
{
    Task AddAsync(UserSession userSession);
    Task UpdateAsync(UserSession userSession);
    Task RemoveAsync(Guid sessionId);
    Task RemoveByUserIdAsync(Guid userId);
}