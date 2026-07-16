namespace Govor.Application.Users.UserOnlineStatus;

public interface IUserNotificationScopeService
{
    Task<List<Guid>> GetNotifiedUsers(Guid userId);
}