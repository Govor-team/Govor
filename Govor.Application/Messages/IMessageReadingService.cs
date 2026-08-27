using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using SmartRes;

namespace Govor.Application.Messages;

public interface IMessageReadingService
{
    Task<Result<Message, Error>> ReadMessageAsync(Guid readerId, Guid messageId);
}