using Govor.Domain.Models.Users.Crypto;

namespace Govor.Application.Users.UserSessions.Crypto;

public interface ISessionKeysReader
{
    Task<bool> HasKeysAttachedAsync(Guid sessionId);
    Task<IReadOnlyList<UserCryptoSession>> GetAllActiveKeysAsync(Guid userId);
    Task<int> GetRemainingOneTimePreKeysCountAsync(Guid sessionId);
    Task<UserCryptoSession?> GetKeysBySessionIdAsync(Guid sessionId);
}