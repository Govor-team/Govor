using Govor.Domain.Common;
using Govor.Domain.Models.Reactions;
using SmartRes;

namespace Govor.Application.Reactions;

public interface IReactionPackService
{
    Task<List<ReactionPack>> ListAsync(Guid userId, bool installedOnly = false, bool includeDisabled = false, int skip = 0, int take = 50);
    Task<Result<ReactionPack, Error>> GetAsync(Guid packId, bool includeDisabled = false);
    Task<Result<ReactionPack, Error>> GetSharedAsync(string shareCode);
    Task<Result<ReactionItem, Error>> GetReactionAsync(Guid reactionId);
    Task<Result<Unit, Error>> SubscribeAsync(Guid userId, Guid packId, bool subscribe);
    Task<Result<ReactionPack, Error>> CreateAsync(Guid actorId, string name, string description);
    Task<Result<ReactionPack, Error>> UpdateAsync(Guid actorId, Guid packId, string name, string description, bool enabled);
    Task<Result<Unit, Error>> DisableAsync(Guid actorId, Guid packId);
    Task<Result<ReactionItem, Error>> AddEmojiAsync(Guid actorId, Guid packId, string name, string emoji);
    Task<Result<ReactionItem, Error>> AddMediaAsync(Guid actorId, Guid packId, string name, byte[] data, CancellationToken cancellationToken = default);
    Task<Result<Unit, Error>> DisableReactionAsync(Guid actorId, Guid packId, Guid reactionId);
}
