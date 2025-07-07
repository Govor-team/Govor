namespace Govor.Core.Models;

public class PrivateChat
{
    public Guid Id { get; set; }
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public List<Message> Messages { get; set; } =  new List<Message>();

    public override bool Equals(object? obj)
    {
        PrivateChat other = obj as PrivateChat;
        return Id == other.Id &&
               UserAId == other.UserAId &&
               UserBId == other.UserBId;
    }
}