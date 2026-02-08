using Govor.Contracts.Responses.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public interface IChatNotificationService
{
    Task NotifyMessageSentAsync(UserMessageResponse message);
    Task NotifyMessageRemovedAsync(MessageRemovedResponse response);
    Task NotifyMessageEditedAsync(MessageEditResponse response);
}