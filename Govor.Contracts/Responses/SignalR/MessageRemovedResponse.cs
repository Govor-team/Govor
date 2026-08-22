using Govor.Contracts.Requests.SignalR;
using Govor.Domain.Models.Messages;

namespace Govor.Contracts.Responses.SignalR;

public class MessageRemovedResponse
{
    public required Guid MessageId { get; set; }
    public required Guid SenderId { get; set; }
    public required Guid RecipientId { get; set; }
    public RemoveMessageRequestType  RequestType { get; set; }
    public RecipientType RecipientType { get; set; }
}