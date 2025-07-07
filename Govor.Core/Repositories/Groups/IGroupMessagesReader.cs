using Govor.Core.Models;

namespace Govor.Core.Repositories.Groups;

public interface IGroupMessagesReader
{
    public Task<IEnumerable<Message>> GetMessages(Guid chatId, Guid? startMessageId, int pageSize = 20);
}