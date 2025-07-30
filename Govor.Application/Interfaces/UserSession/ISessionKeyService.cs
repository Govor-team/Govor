namespace Govor.Application.Interfaces.UserSession;

public interface ISessionKeyService
{
    /// <summary>
    /// Привязать публичные ключи текущего клиента к сессии.
    /// </summary>
    Task AttachKeysAsync(Guid userId, Guid sessionId, string publicEncryptionKey, string publicSigningKey);
    
    Task<SessionPublicKeys?> GetKeysAsync(Guid userId, Guid sessionId);

    /// <summary>
    /// Получить публичные ключи пользователя (например, последнюю активную сессию или все активные).
    /// </summary>
    Task<IEnumerable<SessionPublicKeys>> GetAllActiveKeysAsync(Guid userId);
}

public class SessionPublicKeys
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string PublicEncryptionKey { get; set; } = string.Empty;
    public string PublicSigningKey { get; set; } = string.Empty;
}