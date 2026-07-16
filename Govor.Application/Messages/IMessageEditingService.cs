using Govor.Application.Messages.Parameters;

namespace Govor.Application.Messages;

public interface IMessageEditingService
{
    Task<EditMessageResult> EditMessageAsync(EditMessage editParams);
}