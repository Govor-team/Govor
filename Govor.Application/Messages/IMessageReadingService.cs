using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using SmartRes;
using ChatReadResult = Govor.Application.Messages.Parameters.ChatReadResult;

namespace Govor.Application.Messages;

public interface IMessageReadingService
{
    Task<Result<Message, Error>> ReadMessageAsync(Guid readerId, Guid messageId);
    Task<Result<ChatReadResult, Error>> ReadChatAsync(Guid readerId, Guid chatId,
        RecipientType type, IReadOnlyCollection<Guid>? messageIds = null, Guid? upToMessageId = null);
    Task<Result<int, Error>> GetUnreadCountAsync(Guid readerId, Guid chatId, RecipientType type);
}
