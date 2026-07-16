using Govor.Domain.Models.Messages;

namespace Govor.Application.Messages.Parameters;

public record SendMessageResult(bool IsSuccess, Exception? Exception, Message Message) 
    : Result(IsSuccess, Exception, Message?.Id ?? Guid.Empty);