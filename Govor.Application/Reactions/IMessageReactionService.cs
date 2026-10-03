using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Reactions;
using SmartRes;

namespace Govor.Application.Reactions;

public record ReactionCount(Guid? ReactionId, string ReactionCode, int Count);
public record MessageReactionState(Guid MessageId, Guid RecipientId, RecipientType RecipientType,
    long Version, Guid ActorId, MessageReaction? ActorReaction, IReadOnlyList<ReactionCount> Counts, bool Changed = false);
public record ChannelReactionPolicy(Guid GroupId, ChannelReactionMode Mode, long Version, IReadOnlyList<Guid> ReactionIds);

public interface IMessageReactionService
{
    Task<Result<MessageReactionState, Error>> GetAsync(Guid userId, Guid messageId);
    Task<Result<MessageReactionState, Error>> SetAsync(Guid userId, Guid messageId, Guid? reactionId);
    Task<Result<ChannelReactionPolicy, Error>> GetPolicyAsync(Guid userId, Guid groupId);
    Task<Result<ChannelReactionPolicy, Error>> SetPolicyAsync(Guid userId, Guid groupId, ChannelReactionMode mode, IReadOnlyCollection<Guid> reactionIds);
}
