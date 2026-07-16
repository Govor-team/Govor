namespace Govor.Application.Messages.Parameters;

public record Result(bool IsSuccess, Exception Exception, Guid messageId);