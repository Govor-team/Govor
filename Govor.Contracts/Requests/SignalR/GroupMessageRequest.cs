namespace Govor.Contracts.Requests.SignalR;

public record GroupMessageRequest()
{
    public Guid GroupId { get; init; }
    public string EncryptedContent { get; init; } = string.Empty;
    public Guid? ReplyToMessageId { get; set; }
    public List<MediaReference> MediaAttachments { get; set; } = new();
}