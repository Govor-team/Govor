using Govor.Core.Models;

namespace Govor.Core.Repositories.Groups;

public interface IGroupMessagesReader
{
    public Task<List<Message>> GetMessages(Guid chatId, Guid? startMessageId, int pageSize = 20, RecipientType type = RecipientType.User);
}