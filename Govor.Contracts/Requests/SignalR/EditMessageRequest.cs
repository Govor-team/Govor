namespace Govor.Contracts.Requests.SignalR;

public class EditMessageRequest
{
    public Guid MessageId { get; set; }
    public string NewEncryptedContent { get; set; } = string.Empty;
}
