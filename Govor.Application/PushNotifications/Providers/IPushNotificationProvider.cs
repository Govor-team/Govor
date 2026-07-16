using Govor.Application.Interfaces.PushNotifications.Models;

namespace Govor.Application.PushNotifications.Providers;

public interface IPushNotificationProvider
{
    string Name { get; }
    
    Task<SendPushResult> SendToTokenAsync(string token, PushMessage message);
    
    Task<SendPushResult> SendMulticastAsync(
        IReadOnlyList<string> tokens,
        PushMessage message);
}