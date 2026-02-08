namespace Govor.Application.Interfaces.UserSession;


public interface IUserSessionRevoker
{
    Task CloseSessionByIdAsync(Guid sessionId, Guid userId);
    Task CloseAllSessionsAsync(Guid userId);
}
