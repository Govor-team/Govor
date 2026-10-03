using Govor.Domain.Models.Reactions;

namespace Govor.Contracts.Responses;

public class ReactionPackResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShareCode { get; set; } = string.Empty;
    public string SharePath => $"/api/reaction-packs/shared/{ShareCode}";
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReactionItemResponse> Reactions { get; set; } = [];
}

public class ReactionItemResponse
{
    public Guid Id { get; set; }
    public Guid PackId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ReactionKind Kind { get; set; }
    public string? Emoji { get; set; }
    public Guid? MediaFileId { get; set; }
    public string? MediaUrl => MediaFileId.HasValue ? $"/api/media/download/{MediaFileId}" : null;
    public int Width { get; set; }
    public int Height { get; set; }
    public int SizeBytes { get; set; }
    public int DurationMilliseconds { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
