using Govor.Domain.Models.Messages;

namespace Govor.Application.Messages.Parameters;

public record ChatReadResult(Guid ChatId, RecipientType RecipientType, Guid ReaderId,
    DateTime ReadAt, Guid? UpToMessageId, DateTime? UpToSentAt,
    IReadOnlyList<Guid>? MessageIds, int ReadCount, int UnreadCount);
