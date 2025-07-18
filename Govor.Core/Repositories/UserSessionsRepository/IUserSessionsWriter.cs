using Govor.Core.Models;

namespace Govor.Core.Repositories.UserSessionsRepository;

public interface IUserSessionsWriter
{
    Task AddAsync(UserSession userSession);
    Task UpdateAsync(UserSession userSession);
    Task RemoveAsync(Guid sessionId);
    Task RemoveByUserIdAsync(Guid userId);
}