using System.ComponentModel.DataAnnotations;
using Govor.Domain.Models.Reactions;

namespace Govor.Contracts.Requests;

public class SetReactionRequest
{
    public Guid ReactionId { get; set; }
}

public class CreateReactionPackRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
}

public class UpdateReactionPackRequest : CreateReactionPackRequest
{
    public bool IsEnabled { get; set; } = true;
}

public class CreateEmojiReactionRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string Emoji { get; set; } = string.Empty;
}

public class ChannelReactionPolicyRequest
{
    [EnumDataType(typeof(ChannelReactionMode))] public ChannelReactionMode Mode { get; set; }
    [MaxLength(100)] public List<Guid> ReactionIds { get; set; } = [];
}
