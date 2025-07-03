using Govor.Application.Interfaces.Messages.Parameters;

namespace Govor.Application.Interfaces.Messages;

public interface IMessageManagementService
{
    Task<Result> EditMessageAsync(Guid editorId, Guid messageId, string newContent);
    Task<Result> DeleteMessageAsync(Guid editorId, Guid messageId);
}