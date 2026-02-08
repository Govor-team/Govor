using Govor.Core.Models.Messages;

namespace Govor.Core.Repositories.Messages;

public interface IMessagesWriter
{
    Task AddAsync(Message message);
    Task UpdateAsync(Message message);
    Task RemoveAsync(Guid messageId);
}