namespace Govor.Application.Interfaces.UserOnlineStatus;

public interface IUserPresenceReader
{
    Task<DateTime?> GetLastSeenAsync(Guid userId);
}