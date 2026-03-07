namespace Govor.Application.Interfaces.PushNotifications;

public interface IPushNotificationService
{
    Task SendToUserAsync(Guid userId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);

    Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);
    
    Task SendToSessionAsync(Guid sessionId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);
}