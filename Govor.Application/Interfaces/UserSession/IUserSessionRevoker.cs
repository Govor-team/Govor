namespace Govor.Application.Interfaces.UserSession;


public interface IUserSessionRevoker
{
    Task CloseSessionByRefreshTokenAsync(string refreshToken);
    Task CloseAllSessionsAsync(Guid userId);
}
