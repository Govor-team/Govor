namespace Govor.Application.Interfaces.UserOnlineStatus;

public interface IOnlineUserStore
{
    void SetOnlineUser(Guid userId);
    void SetOfflineUser(Guid userId);
    bool IsOnline(Guid userId);
    IReadOnlyCollection<Guid> GetAllOnlineUsers();
}