namespace Govor.Application.Users.UserOnlineStatus;

public class UserPresenceReader : IUserPresenceReader
{
    public Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}