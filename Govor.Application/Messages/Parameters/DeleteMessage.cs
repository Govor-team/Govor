namespace Govor.Application.Messages.Parameters;

public record DeleteMessage(
    Guid DeleterId,
    Guid MessageId);