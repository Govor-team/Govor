namespace Govor.Domain.Models.Messages;

public class ChatPushNotification
{
    public Guid MessageId { get; set; }
    public DateTime NextAttemptAt { get; set; }
    public int Attempts { get; set; }
}
