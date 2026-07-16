namespace Govor.Application.Messages.Parameters;

public record EditMessage(
    Guid EditorId,
    Guid MessageId,
    string NewContent,
    DateTime EditedAt);