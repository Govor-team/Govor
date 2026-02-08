namespace Govor.Application.Interfaces.UserOnlineStatus;

public interface IUserNotificationScopeService
{
    Task<List<Guid>> GetNotifiedUsers(Guid userId);
}