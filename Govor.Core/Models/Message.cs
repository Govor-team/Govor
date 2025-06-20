namespace Govor.Core.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; } // or GroupId
    public RecipientType RecipientType { get; set; }
    public string EncryptedContent { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
}

public enum RecipientType
{
    User,
    Group
}