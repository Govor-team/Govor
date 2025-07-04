namespace Govor.Application.Interfaces.Messages.Parameters;

public record DeleteMessage(
    Guid DeleterId,
    Guid MessageId);
