namespace Govor.Core.Models;

public class MediaAttachments
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public MediaType Type { get; set; }
    public string FilePath { get; set; } = string.Empty; // local path in filesystem 
    public string MimeType { get; set; } = string.Empty;
    public string? EncryptedKey { get; set; } 
    public Message Message { get; set; } = null!;
    
    public override bool Equals(object? obj)
    {
        if (obj is not MediaAttachments other) return false;

        return Id == other.Id &&
               MessageId == other.MessageId &&
               EncryptedKey == other.EncryptedKey &&
               Type == other.Type && 
               FilePath == other.FilePath &&
               MimeType == other.MimeType;
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