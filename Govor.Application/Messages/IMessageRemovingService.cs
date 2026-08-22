using Govor.Application.Messages.Parameters;
using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using SmartRes;

namespace Govor.Application.Messages;

public interface IMessageRemovingService
{
    Task<Result<Message,Error>> DeleteMessageAsync(DeleteMessage deleteParams);
}