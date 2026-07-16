using Govor.Domain.Common;

namespace Govor.Application.PushNotifications;

public interface IPushTokenService
{
    Task<Result> DeactivateTokenBySessionAsync(Guid sessionId);
    Task<Result> DeactivateAllTokensByUserIdAsync(Guid userId);
    Task<Result<List<string>>> GetStringsActiveTokensAsync(Guid userId);
    Task<Result<List<string>>> GetUsersStringsActiveTokensAsync(IEnumerable<Guid> userIds);
    Task<Result<string?>> GetActiveTokenBySessionAsync(Guid sessionId);
    Task<Result> RemoveTokensAsync(IEnumerable<string> tokens);
    Task<Result> AddOrUpdateTokenAsync(Guid userId, Guid sessionId, string token, string platform);
}