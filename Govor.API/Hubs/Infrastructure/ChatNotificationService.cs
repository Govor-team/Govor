using Govor.Application.PrivateUserChats;
using Govor.Application.Profiles;
using Govor.Application.PushNotifications;
using Govor.Contracts.Responses.SignalR;
using Govor.Domain.Models.Messages;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public class ChatNotificationService : IChatNotificationService 
{
    private readonly IHubContext<ChatsHub> _hubContext;
    private readonly IPushNotificationService _notificationService;
    private readonly IUserPrivateChatsGetterService _userPrivateChatsGetterService;
    private readonly IProfileService _profileService;

    public ChatNotificationService(
        IHubContext<ChatsHub> hubContext,
        IPushNotificationService notificationService,
        IUserPrivateChatsGetterService userPrivateChatsGetterService, 
        IProfileService profileService)
    {
        _hubContext = hubContext;
        _notificationService = notificationService;
        _userPrivateChatsGetterService = userPrivateChatsGetterService;
        _profileService = profileService;
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

    public async Task NotifyMessageWasReadAsync(MessageReadResponse response)
    {
        await NotifyParticipantsAsync(
            response.ReaderId, 
            response.RecipientId, 
            response.RecipientType, 
            ChatHubConstants.MessageRead, 
            response);
    }

    private async Task NotifyMessageReceivedInPrivateChatAsync(UserMessageResponse message)
    {
        
        var result = await _userPrivateChatsGetterService.GetPrivateChatAsync(message.RecipientId);
        
        if(result.IsFailure)
           return;
        var privateChat = result.Value;
        
        var text = message.EncryptedContent.Substring(0, Math.Min(40, message.EncryptedContent.Length));
        var userId = message.SenderId == privateChat.UserAId ? privateChat.UserBId : privateChat.UserAId;
       
        var resultProf = await _profileService.GetUserProfileAsync(message.SenderId);
        
        if(resultProf.IsFailure)
            return;
        var profile = resultProf.Value;
        
        var title = profile.Username;
        Dictionary<string, string> data = new Dictionary<string, string>();
        
        data["chatId"] = message.RecipientId.ToString();
        data["isGroup"] = message.RecipientType == RecipientType.Group ? "true" : "false";
        
        await _notificationService.SendToUserAsync(
            userId, 
            title,
            text,  
            "chat_messages",
            $"private_chat_{message.RecipientId}",
            data);
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