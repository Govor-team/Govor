namespace Govor.Application.Users.UserOnlineStatus;

public interface IUserPresenceReader
{
    Task<DateTime?> GetLastSeenAsync(Guid userId);
}