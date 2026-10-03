using Govor.Contracts.Responses.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public interface IChatNotificationService
{
    Task NotifyMessageSentAsync(UserMessageResponse message);
    Task<bool> DeliverPushAsync(UserMessageResponse message);
    Task NotifyMessageWasReadAsync(MessageReadResponse response);
    Task NotifyChatReadAsync(ChatReadResponse response);
    Task NotifyMessageReactionsChangedAsync(MessageReactionsChangedResponse response);
    Task NotifyChannelReactionPolicyChangedAsync(ChannelReactionPolicyResponse response);
    Task NotifyMessageRemovedAsync(MessageRemovedResponse response);
    Task NotifyMessageEditedAsync(MessageEditResponse response);
}
