namespace Govor.Application.Interfaces.Messages.Parameters;

public record EditMessage(
    Guid EditorId,
    Guid MessageId,
    string NewContent,
    DateTime EditedAt);
