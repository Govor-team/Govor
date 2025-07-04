using Govor.Core.Models;

namespace Govor.Contracts.Requests.SignalR;

public record MessageRequest
{
    public Guid RecipientId { get; init; }
    public RecipientType RecipientType { get; init; } // Added this field
    public string EncryptedContent { get; init; } = string.Empty;
    public Guid? ReplyToMessageId { get; set; }
    public List<MediaReference> MediaAttachments { get; set; } = new();
}