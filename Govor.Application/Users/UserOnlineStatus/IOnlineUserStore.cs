namespace Govor.Application.Users.UserOnlineStatus;

public interface IOnlineUserStore
{
    bool AddConnection(Guid userId, string connectionId);
    bool RemoveConnection(Guid userId, string connectionId);
    bool IsOnline(Guid userId);
    IEnumerable<string> GetConnections(Guid userId);
    IReadOnlyCollection<Guid> GetAllOnlineUsers();
}