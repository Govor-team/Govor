namespace Govor.Core.Repositories.PushTokens;

public interface IPushTokenWriter
{
    Task AddOrUpdateTokenAsync(Guid userId, Guid? sessionId, string token, 
        string platform, string provider = "FCM");

    Task RemoveTokensAsync(IEnumerable<string> tokens);

    Task DeactivateTokenBySessionAsync(Guid sessionId); // logout
}