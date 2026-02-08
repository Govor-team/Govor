namespace Govor.Application.Interfaces.Messages.Parameters;

public record Result(bool IsSuccess, Exception Exception, Guid messageId);