namespace Govor.Core.Models;

public class MediaAttachments
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }

    public MediaType Type { get; set; }
    public string FilePath { get; set; } = string.Empty; // путь к файлу (локальный или URL)
    public string MimeType { get; set; } = string.Empty;

    public string? EncryptedKey { get; set; } // если используется отдельное шифрование
    public Message Message { get; set; } = null!;
}

public enum MediaType
{
    Image,
    Video,
    Audio,
    File,
    Voice
}