using Govor.Contracts.Requests.SignalR;

namespace Govor.Contracts.Responses.SignalR;

public record UserMessageResponse
{
    public Guid Id { get; init; }
    public Guid SenderId { get; init; }
    public string EncryptedContent { get; init; } = string.Empty;
    public Guid? ReplyToMessageId { get; set; }
    public List<MediaReference> MediaReferences { get; init; } = new List<MediaReference>();
}