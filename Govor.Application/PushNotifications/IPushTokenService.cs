using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.PushNotifications;

public interface IPushTokenService
{
    Task<Result<Unit, Error>>  DeactivateTokenBySessionAsync(Guid sessionId);
    Task<Result<Unit, Error>>  DeactivateAllTokensByUserIdAsync(Guid userId);
    Task<Result<List<string>, Error>> GetStringsActiveTokensAsync(Guid userId);
    Task<Result<List<string>, Error>> GetUsersStringsActiveTokensAsync(IEnumerable<Guid> userIds);
    Task<Result<string?, Error>> GetActiveTokenBySessionAsync(Guid sessionId);
    Task<Result<Unit, Error>> RemoveTokensAsync(IEnumerable<string> tokens);
    Task<Result<Unit, Error>> AddOrUpdateTokenAsync(Guid userId, Guid sessionId, string token, string platform);
}