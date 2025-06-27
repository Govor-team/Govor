namespace Govor.Core.Models;

public class PrivateChat
{
    public Guid Id { get; set; }
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public List<Message> Messages { get; set; } =  new List<Message>();
}