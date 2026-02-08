using Govor.Application.Interfaces.UserOnlineStatus;

namespace Govor.Application.Services.UserOnlineStatus;

public class UserPresenceReader : IUserPresenceReader
{
    public Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}