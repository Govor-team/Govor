namespace Govor.Application.Interfaces.PushNotifications.Models;

public record SendPushResult(int SuccessCount, int FailureCount, IEnumerable<string> FailedTokens);