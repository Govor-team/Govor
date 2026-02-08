using Govor.Core.Models.Messages;

namespace Govor.Core.Repositories.Messages;

public interface IMessagesExist
{
    bool Exist(Guid messageId);
    bool Exist(Message message);
}