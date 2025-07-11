using Govor.Core.Models.Messages;

namespace Govor.Application.Interfaces;

public interface IMessagesLoader
{
    Task<List<Message>> LoadLastMessagesInUserChat(Guid userId,Guid currentId, Guid? startMessageId, int pageSize = 20);
    Task<List<Message>> LoadLastMessagesInChatGroup(Guid chatId,Guid currentId, Guid? startMessageId, int pageSize = 20);
}