using Govor.Core.Models; // For RecipientType

namespace Govor.Contracts.Responses.SignalR;

public class MessageRemovedResponse
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; } // Original Sender
    public Guid RecipientId { get; set; }
    public RecipientType RecipientType { get; set; }
}
