using System.Collections.Concurrent;
using Govor.Application.Interfaces.UserOnlineStatus;

namespace Govor.Application.Services.UserOnlineStatus;

public class OnlineUserStore : IOnlineUserStore
{
    private readonly ConcurrentDictionary<Guid, DateTime> _onlineUsers = new();

    public void SetOnlineUser(Guid userId)
    {
        _onlineUsers[userId] = DateTime.UtcNow;
    }

    public void SetOfflineUser(Guid userId)
    {
        _onlineUsers.TryRemove(userId, out _);
    }

    public bool IsOnline(Guid userId)
    {
        return _onlineUsers.ContainsKey(userId);
    }

    public IReadOnlyCollection<Guid> GetAllOnlineUsers()
    {
        return _onlineUsers.Keys.ToList();
    }
}
