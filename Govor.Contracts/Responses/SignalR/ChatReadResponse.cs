using Govor.Domain.Models.Messages;

namespace Govor.Contracts.Responses.SignalR;

public class ChatReadResponse
{
    public Guid ChatId { get; set; }
    public RecipientType RecipientType { get; set; }
    public Guid ReaderId { get; set; }
    public DateTime ReadAt { get; set; }
    public Guid? UpToMessageId { get; set; }
    public DateTime? UpToSentAt { get; set; }
    public IReadOnlyList<Guid>? MessageIds { get; set; }
    public int ReadCount { get; set; }
    public int UnreadCount { get; set; }
}
