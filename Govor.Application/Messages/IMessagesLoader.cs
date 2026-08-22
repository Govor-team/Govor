using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using SmartRes;

namespace Govor.Application.Messages;

public interface IMessagesLoader
{
    Task<Result<List<Message>,Error>> LoadMessagesInUserChat(Guid privateChatId,Guid currentId, Guid? startMessageId, int before = 20, int after = 2);
    Task<Result<List<Message>,Error>> LoadMessagesInChatGroup(Guid chatId,Guid currentId, Guid? startMessageId, int before = 20, int after = 2);
}