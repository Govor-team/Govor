using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Reactions;

namespace Govor.Contracts.Responses.SignalR;

public class ReactionCountResponse
{
    public Guid? ReactionId { get; set; }
    public string ReactionCode { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MessageReactionsChangedResponse
{
    public Guid MessageId { get; set; }
    public Guid RecipientId { get; set; }
    public RecipientType RecipientType { get; set; }
    public long Version { get; set; }
    public Guid ActorId { get; set; }
    public MessageReactionResponse? ActorReaction { get; set; }
    public List<ReactionCountResponse> Counts { get; set; } = [];
}

public class ChannelReactionPolicyResponse
{
    public Guid GroupId { get; set; }
    public ChannelReactionMode Mode { get; set; }
    public long Version { get; set; }
    public List<Guid> ReactionIds { get; set; } = [];
}
