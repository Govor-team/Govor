using Govor.Core.Models.Messages;

namespace Govor.Contracts.Responses.SignalR;

public class MessageRemovedResponse
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public RecipientType RecipientType { get; set; }
}