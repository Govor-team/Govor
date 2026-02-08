namespace Govor.Application.Interfaces.UserSession;

public interface IUserSessionReader
{
    Task<List<Core.Models.Users.UserSession>> GetAllSessionsAsync(Guid userId);
}
