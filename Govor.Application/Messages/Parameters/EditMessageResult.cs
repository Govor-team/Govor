using Govor.Domain.Models.Messages;

namespace Govor.Application.Messages.Parameters;

public record EditMessageResult(bool IsSuccess, Exception? Exception, Message? OriginalMessage) 
    : Result(IsSuccess, Exception, OriginalMessage?.Id ?? Guid.Empty)
{

}