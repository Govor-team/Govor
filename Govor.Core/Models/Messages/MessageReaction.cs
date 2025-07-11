using Govor.Core.Models.Users;

namespace Govor.Core.Models.Messages;

public class MessageReaction
{
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    public Message Message { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } 
    
    public string ReactionCode { get; set; } // "❤️", "🔥", "👍", ":custom_emoji:" 
    public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
}