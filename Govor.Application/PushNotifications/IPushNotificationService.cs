using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.PushNotifications;

public interface IPushNotificationService
{
    Task<Result<Unit, Error>> SendToUserAsync(Guid userId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);

    Task<Result<Unit, Error>> SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);
    
    Task<Result<Unit, Error>> SendToSessionAsync(Guid sessionId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null);
}