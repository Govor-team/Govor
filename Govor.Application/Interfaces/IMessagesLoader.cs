using Govor.Core.Models.Messages;

namespace Govor.Application.Interfaces;

public interface IMessagesLoader
{
    Task<List<Message>> LoadMessagesInUserChat(Guid userId,Guid currentId, Guid? startMessageId, int before = 20, int after = 2);
    Task<List<Message>> LoadMessagesInChatGroup(Guid chatId,Guid currentId, Guid? startMessageId, int before = 20, int after = 2);
}