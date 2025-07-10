using Govor.Application.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.Messages;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services.Messages;

public class MessagesLoader : IMessagesLoader
{
    private IVerifyFriendship _friendship;
    private IGroupsRepository _groupsRepository;
    private IMessagesRepository _messagesRepository;
    
    public async Task<List<Message>> LoadLastMessagesInUserChat(Guid userId, Guid currentUser, Guid? startMessageId, int pageSize = 20)
    {
        await _friendship.VerifyAsync(userId, currentUser);
        try
        {
            throw new NotImplementedException();
            //return await _groups.GetMessages(userId, startMessageId, pageSize);
        }
        catch (NotFoundException ex)
        {
            return new List<Message>();
        }
    }

    public async Task<List<Message>> LoadLastMessagesInChatGroup(Guid chatId, Guid currentUser, Guid? startMessageId, int pageSize = 20)
    {
        if(!await _groupsRepository.IsUserMemberOfGroupAsync(currentUser, chatId))
            throw new UnauthorizedAccessException("You are not in a group.");
        try
        {
            throw new NotImplementedException();
            //return await _groups.GetMessages(chatId, startMessageId, pageSize, RecipientType.Group);
        }
        catch (NotFoundException ex)
        {
            return new List<Message>();
        }
    }
}