using Govor.Domain.Common;

namespace Govor.Application.PushNotifications;

public interface IPushNotificationService
{
    Task<Result> SendToUserAsync(Guid userId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);

    Task<Result> SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);
    
    Task<Result> SendToSessionAsync(Guid sessionId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);
}