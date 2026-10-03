namespace Govor.Domain.Models.Reactions;

public enum ReactionKind { Emoji, Image, Gif }
public enum ChannelReactionMode { All, None, Selected }

public class ReactionItem
{
    public Guid Id { get; set; }
    public Guid PackId { get; set; }
    public ReactionPack Pack { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ReactionKind Kind { get; set; }
    public string? Emoji { get; set; }
    public Guid? MediaFileId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SizeBytes { get; set; }
    public int DurationMilliseconds { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class UserReactionPack
{
    public Guid UserId { get; set; }
    public Guid PackId { get; set; }
}

public class ChannelAllowedReaction
{
    public Guid GroupId { get; set; }
    public Guid ReactionId { get; set; }
}
