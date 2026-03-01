namespace Govor.Core.Repositories.PushTokens;

public interface IPushTokenReader
{
    Task<List<string>> GetActiveTokensAsync(Guid userId);
    
    Task<List<string>> GetActiveTokensUsersAsync(IEnumerable<Guid> userIds);

    Task<string> GetActiveTokenBySessionAsync(Guid sessionId);
}