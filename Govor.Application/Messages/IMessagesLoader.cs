using Govor.Domain.Models.Messages;

namespace Govor.Application.Messages;

public interface IMessagesLoader
{
    Task<List<Message>> LoadMessagesInUserChat(Guid privateChatId,Guid currentId, Guid? startMessageId, int before = 20, int after = 2);
    Task<List<Message>> LoadMessagesInChatGroup(Guid chatId,Guid currentId, Guid? startMessageId, int before = 20, int after = 2);
}