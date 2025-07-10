using Govor.Application.Interfaces.Friends;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services.Friends;

public class FriendRequestQueryService : IFriendRequestQueryService
{
    private readonly IFriendshipsRepository _friendshipsRepository;

    public FriendRequestQueryService(IFriendshipsRepository friendshipsRepository)
    {
        _friendshipsRepository = friendshipsRepository;
    }

    public async Task<List<Friendship>> GetIncomingAsync(Guid userId)
    {
        try
        {
            var friendships = await _friendshipsRepository.FindByUserIdAsync(userId);
            return friendships.Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending).ToList()
                   ?? new List<Friendship>();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("User not exist", ex);
        }
    }

    public async Task<List<Friendship>> GetResponsesAsync(Guid userId)
    {
        try
        {
            var friendships = await _friendshipsRepository.FindByUserIdAsync(userId);
            return friendships.Where(f => f.RequesterId == userId && f.Status != FriendshipStatus.Accepted).ToList()
                   ?? new List<Friendship>();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("User not exist", ex);
        }
    }
}