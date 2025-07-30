namespace Govor.Application.Interfaces.UserSession.Crypto;

public interface IOneTimePreKeysRotator
{
    Task RotateOneTimePreKeysAsync(Guid sessionId, IEnumerable<byte[]> newOneTimePreKeys);
    
    Task MarkOneTimePreKeyAsUsedAsync(Guid sessionId, Guid oneTimePreKeyId);
}