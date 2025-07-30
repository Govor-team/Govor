using Govor.Core.Models.Users.Crypto;

namespace Govor.Application.Interfaces.UserSession.Crypto;

public interface ISessionKeysReader
{
    Task<bool> HasKeysAttachedAsync(Guid sessionId);
    Task<IReadOnlyList<UserCryptoSession>> GetAllActiveKeysAsync(Guid userId);
    Task<int> GetRemainingOneTimePreKeysCountAsync(Guid sessionId);
    Task<UserCryptoSession?> GetKeysBySessionIdAsync(Guid sessionId);
}