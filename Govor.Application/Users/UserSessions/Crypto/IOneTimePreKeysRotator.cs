namespace Govor.Application.Users.UserSessions.Crypto;

public interface IOneTimePreKeysRotator
{
    Task RotateOneTimePreKeysAsync(Guid sessionId, IEnumerable<byte[]> newOneTimePreKeys);
    
    Task MarkOneTimePreKeyAsUsedAsync(Guid sessionId, Guid oneTimePreKeyId);
}