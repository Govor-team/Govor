namespace Govor.Domain.Models.Messages;

public class MediaAttachments
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid MediaFileId { get; set; }
    public Message Message { get; set; }
    public MediaFile MediaFile { get; set; }
    public override bool Equals(object? obj)
    {
        if (obj is not MediaAttachments other) return false;

        return Id == other.Id &&
               MessageId == other.MessageId &&
               MediaFileId == other.MediaFileId;
    }
}

public enum MediaType
{
    Image,
    Video,
    Audio,
    File,
    Voice
}