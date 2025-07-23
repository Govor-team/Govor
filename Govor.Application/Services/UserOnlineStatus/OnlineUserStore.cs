using Govor.Application.Interfaces.UserOnlineStatus;

namespace Govor.Application.Services.UserOnlineStatus;

public class OnlineUserStore : IOnlineUserStore
{
    public void SetOnlineUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    public void SetOfflineUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    public bool IsOnline(Guid userId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<Guid> GetAllOnlineUsers()
    {
        throw new NotImplementedException();
    }
}