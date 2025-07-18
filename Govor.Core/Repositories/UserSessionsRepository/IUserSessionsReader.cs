using Govor.Core.Models;

namespace Govor.Core.Repositories.UserSessionsRepository;

public interface IUserSessionsReader
{
    public Task<List<UserSession>> GetAllAsync();
    public Task<UserSession> GetByIdAsync(Guid sessionId);
    public Task<List<UserSession>> GetByUserIdAsync(Guid userId);
    public Task<List<UserSession>> GetByCreatedAtAsync(DateTime createdAt);
    public Task<List<UserSession>> GetByExpiresAtAsync(DateTime createdAt);
    public Task<List<UserSession>> GetByRevokedAsync(bool isRevoked);
}