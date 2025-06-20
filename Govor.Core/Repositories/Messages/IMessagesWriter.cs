using Govor.Core.Models;

namespace Govor.Core.Repositories.Messages;

public interface IMessagesWriter
{
    void Add(Message message);
    void Update(Message message);
    void Delete(Guid messageId);
}