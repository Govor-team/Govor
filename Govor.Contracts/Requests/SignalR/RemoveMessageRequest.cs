namespace Govor.Contracts.Requests.SignalR;

public class RemoveMessageRequest
{
    public Guid MessageId { get; set; }
    public RemoveMessageRequestType RequestType { get; set; }
}