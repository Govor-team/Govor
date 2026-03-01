using Govor.Application.Interfaces;
using Govor.Application.Interfaces.PushNotifications;
using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.PrivateChats;
using Govor.Core.Repositories.PushTokens;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public class ChatNotificationService : IChatNotificationService 
{
    private readonly IHubContext<ChatsHub> _hubContext;
    private readonly IPrivateChatsRepository _privateChatsRepository;
    private readonly IPushNotificationService _notificationService;
    private readonly IProfileService _profileService;
    public ChatNotificationService(
        IProfileService profileService,
        IHubContext<ChatsHub> hubContext, 
        IPushNotificationService notificationService,
        IPrivateChatsRepository privateChatsRepository)
    {
        _hubContext = hubContext;
        _profileService = profileService;
        _notificationService = notificationService;
        _privateChatsRepository = privateChatsRepository;
    }

    public async Task NotifyMessageSentAsync(UserMessageResponse message)
    {
        if (message.RecipientType == RecipientType.User)
        {
            await _hubContext.Clients.Group(ChatHubConstants.GetPrivateChat(message.RecipientId))
                .SendAsync(ChatHubConstants.ReceiveMessage, message);
            
            await NotifyMessageReceivedInPrivateChatAsync(message);
        }
        else
        {
            await _hubContext.Clients.Group(ChatHubConstants.GetChatGroup(message.RecipientId))
                .SendAsync(ChatHubConstants.ReceiveMessage, message);
        }
        
       // await _hubContext.Clients.Group(ChatHubConstants.GetUserGroup(message.SenderId))
       //     .SendAsync(ChatHubConstants.MessageSent, message);
    }

    private async Task NotifyMessageReceivedInPrivateChatAsync(UserMessageResponse message)
    {
        var privateChat = await _privateChatsRepository.GetByIdAsync(message.RecipientId);
       
        var text = message.EncryptedContent.Substring(0, Math.Min(40, message.EncryptedContent.Length));
        var userId = message.SenderId == privateChat.UserAId ? privateChat.UserAId : privateChat.UserBId;
       
        var profile = await _profileService.GetUserProfileAsync(userId);
        var title = profile.Username;
       
        await _notificationService.SendToUserAsync(userId, title,text, "chat_messages");
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