namespace Govor.Domain.Models.Reactions;

public static class DefaultReactionPack
{
    public static readonly Guid Id = Guid.Parse("b3000000-0000-0000-0000-000000000001");
    private static readonly DateTime CreatedAt = new(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc);
    public static ReactionPack Create() => new()
    {
        Id = Id, Name = "Emoji", Description = "Default public emoji reactions",
        ShareCode = "default", IsDefault = true, IsEnabled = true, CreatedAt = CreatedAt
    };
    public static ReactionItem[] Items() => new[] { "👍", "👎", "❤️", "🔥", "🥰", "👏", "😁", "🤔", "😢", "🎉", "🤯", "🙏" }
        .Select((emoji, index) => new ReactionItem
        {
            Id = Guid.Parse($"b3000000-0000-0000-0001-{index + 1:000000000000}"),
            PackId = Id, Code = emoji, Name = emoji, Emoji = emoji,
            Kind = ReactionKind.Emoji, IsEnabled = true, CreatedAt = CreatedAt
        }).ToArray();
}
