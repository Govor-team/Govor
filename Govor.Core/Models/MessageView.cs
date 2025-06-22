namespace Govor.Core.Models;

public class MessageView
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ViewedAt { get; set; }
}
