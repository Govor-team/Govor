namespace Govor.Application.Interfaces.UserSession;

public interface IUserSessionRefresher
{
     Task<RefreshResult> RefreshTokenAsync(string refreshToken);
}

public record RefreshResult(string refreshToken, string accessToken);