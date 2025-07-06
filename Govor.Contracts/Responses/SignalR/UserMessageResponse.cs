using Govor.Contracts.Requests.SignalR;
using Govor.Core.Models;

namespace Govor.Contracts.Responses.SignalR;

public record UserMessageResponse
{
    public Guid MessageId { get; init; }
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public RecipientType RecipientType{get; init; }
    public string EncryptedContent { get; init; } = string.Empty;
    public Guid? ReplyToMessageId { get; init; }
    public DateTime SentAt { get; init; }
    public bool IsEdited { get; init; } = false;
    public List<MediaFile> MediaAttachments { get; init; } = new List<MediaFile>();
}