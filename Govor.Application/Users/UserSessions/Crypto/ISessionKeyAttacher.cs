using Govor.Domain.Models.Users.Crypto;

namespace Govor.Application.Users.UserSessions.Crypto;

public interface ISessionKeyAttacher
{
    Task AttachKeysAsync(
        Guid sessionId, 
        byte[] identityKey, 
        byte[] signedPreKey, 
        byte[] signedPreKeySignature, 
        IEnumerable<byte[]> oneTimePreKeys);
}