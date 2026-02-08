using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public class ChatNotificationService : IChatNotificationService 
{
private readonly IHubContext<ChatsHub> _hubContext;

    public ChatNotificationService(IHubContext<ChatsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMessageSentAsync(UserMessageResponse message)
    {
        if (message.RecipientType == RecipientType.User)
        {
            await _hubContext.Clients.Group(ChatHubConstants.GetPrivateChat(message.RecipientId))
                .SendAsync(ChatHubConstants.ReceiveMessage, message);
        }
        else
        {
            await _hubContext.Clients.Group(ChatHubConstants.GetChatGroup(message.RecipientId))
                .SendAsync(ChatHubConstants.ReceiveMessage, message);
        }
        
       // await _hubContext.Clients.Group(ChatHubConstants.GetUserGroup(message.SenderId))
       //     .SendAsync(ChatHubConstants.MessageSent, message);
    }

    public async Task NotifyMessageRemovedAsync(MessageRemovedResponse response)
    {
        await NotifyParticipantsAsync(
            response.SenderId, 
            response.RecipientId, 
            response.RecipientType, 
            ChatHubConstants.MessageRemoved, 
            response);
    }

    public async Task NotifyMessageEditedAsync(MessageEditResponse response)
    {
        await NotifyParticipantsAsync(
            response.EditorId, 
            response.RecipientId, 
            response.RecipientType, 
            ChatHubConstants.MessageEdited, 
            response);
    }
    
    private async Task NotifyParticipantsAsync(Guid initiatorId, Guid targetId, RecipientType type, string method, object payload)
    {
        if (type == RecipientType.User)
        {
            await _hubContext.Clients.Group(ChatHubConstants.GetUserGroup(initiatorId))
                .SendAsync(method, payload);
            
            if (initiatorId != targetId)
            {
                await _hubContext.Clients.Group(ChatHubConstants.GetUserGroup(targetId))
                    .SendAsync(method, payload);
            }
        }
        else
        {
            // to groups 
            await _hubContext.Clients.Group(ChatHubConstants.GetChatGroup(targetId))
                .SendAsync(method, payload);
        }
    }
}