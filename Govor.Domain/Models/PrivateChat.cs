using Govor.Domain.Models.Messages;

namespace Govor.Domain.Models;

public class PrivateChat
{
    public Guid Id { get; set; }
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public List<Message> Messages { get; set; } =  new();

    public override bool Equals(object? obj)
    {
        PrivateChat other = obj as PrivateChat;
        return Id == other.Id &&
               UserAId == other.UserAId &&
               UserBId == other.UserBId;
    }
}