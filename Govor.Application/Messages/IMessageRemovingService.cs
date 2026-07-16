using Govor.Application.Messages.Parameters;

namespace Govor.Application.Messages;

public interface IMessageRemovingService
{
    Task<DeleteMessageResult> DeleteMessageAsync(DeleteMessage deleteParams);
}