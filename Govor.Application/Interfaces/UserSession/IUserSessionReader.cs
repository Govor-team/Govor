namespace Govor.Application.Interfaces.UserSession;

public interface IUserSessionReader
{
    Task<List<Core.Models.UserSession>> GetAllSessionsAsync(Guid userId);
}
