using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services;


public class VerifyFriendship : IVerifyFriendship
{
    private readonly IFriendshipsRepository _friendshipsRepository;
    private readonly ILogger<VerifyFriendship> _logger;
    private const string FriendshipNotAcceptedError = "Friendship between user {0} and friend {1} does not exist or is not accepted.";

    public VerifyFriendship(IFriendshipsRepository friendshipsRepository, ILogger<VerifyFriendship> logger = null)
    {
        _friendshipsRepository = friendshipsRepository ?? throw new ArgumentNullException(nameof(friendshipsRepository));
        _logger = logger;
    }

    public async Task VerifyAsync(Guid targetUserId, Guid friendUserId)
    {
        if (targetUserId == Guid.Empty || friendUserId == Guid.Empty)
        {
            _logger?.LogWarning("Invalid user IDs provided: targetUserId={TargetUserId}, friendUserId={FriendUserId}", targetUserId, friendUserId);
            throw new ArgumentException("User IDs cannot be empty.", nameof(targetUserId));
        }

        var friendships = await _friendshipsRepository.FindByUserIdAsync(targetUserId);
        var friendship = friendships.Where(f => f.AddresseeId == friendUserId || f.RequesterId == friendUserId)?.FirstOrDefault();
        
        if (friendship == null || friendship.Status != FriendshipStatus.Accepted)
        {
            var errorMessage = string.Format(FriendshipNotAcceptedError, targetUserId, friendUserId);
            _logger?.LogError(errorMessage);
            throw new FriendshipException(errorMessage);
        }

        _logger?.LogInformation("Friendship verified successfully for targetUserId={TargetUserId}, friendUserId={FriendUserId}", targetUserId, friendUserId);
    }

    public async Task<bool> TryVerifyAsync(Guid targetUserId, Guid friendUserId)
    {
        try
        {
            await VerifyAsync(targetUserId, friendUserId);
            return true;
        }
        catch (FriendshipException ex)
        {
            return false;
        }
    }
}