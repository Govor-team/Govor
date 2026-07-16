using Microsoft.EntityFrameworkCore;
using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Domain;
using Govor.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Friends;

public class VerifyFriendship : IVerifyFriendship
{
    private readonly GovorDbContext _dbContext;
    private readonly ILogger<VerifyFriendship> _logger;
    private const string FriendshipNotAcceptedError = "Friendship between user {0} and friend {1} does not exist or is not accepted.";

    public VerifyFriendship(GovorDbContext dbContext, ILogger<VerifyFriendship> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task VerifyAsync(Guid targetUserId, Guid friendUserId)
    {
        if (targetUserId == Guid.Empty || friendUserId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user IDs provided: targetUserId={TargetUserId}, friendUserId={FriendUserId}", targetUserId, friendUserId);
            throw new ArgumentException("User IDs cannot be empty.");
        }

       
        var isFriendshipAccepted = await _dbContext.Friendships
            .AsNoTracking()
            .AnyAsync(f => f.Status == FriendshipStatus.Accepted &&
                           ((f.RequesterId == targetUserId && f.AddresseeId == friendUserId) ||
                            (f.RequesterId == friendUserId && f.AddresseeId == targetUserId)));

        if (!isFriendshipAccepted)
        {
            var errorMessage = string.Format(FriendshipNotAcceptedError, targetUserId, friendUserId);
            _logger.LogError(errorMessage);
            throw new FriendshipException(errorMessage);
        }

        _logger.LogInformation("Friendship verified successfully for targetUserId={TargetUserId}, friendUserId={FriendUserId}", targetUserId, friendUserId);
    }

    public async Task<bool> TryVerifyAsync(Guid targetUserId, Guid friendUserId)
    {
        if (targetUserId == Guid.Empty || friendUserId == Guid.Empty)
        {
            return false;
        }
        
        return await _dbContext.Friendships
            .AsNoTracking()
            .AnyAsync(f => f.Status == FriendshipStatus.Accepted &&
                           ((f.RequesterId == targetUserId && f.AddresseeId == friendUserId) ||
                            (f.RequesterId == friendUserId && f.AddresseeId == targetUserId)));
    }
}
