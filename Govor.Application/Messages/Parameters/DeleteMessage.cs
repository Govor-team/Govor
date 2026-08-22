namespace Govor.Application.Messages.Parameters;

public record DeleteMessage(
    Guid DeleterId,
    Guid MessageId,
    bool ForceRemove = false);
