using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services.Friends;

public class FriendRequestService : IFriendRequestService
{
    private readonly IFriendshipsRepository _friendshipsRepository;

    public FriendRequestService(IFriendshipsRepository friendshipsRepository)
    {
        _friendshipsRepository = friendshipsRepository;
    }

    public async Task SendFriendRequestAsync(Guid fromUserId, Guid toUserId)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("Cannot send a request to self user");

        if (_friendshipsRepository.Exist(fromUserId, toUserId))
            throw new RequestAlreadySentException(fromUserId, toUserId);

        await _friendshipsRepository.AddAsync(new Friendship
        {
            Id = Guid.NewGuid(),
            RequesterId = fromUserId,
            AddresseeId = toUserId,
            Status = FriendshipStatus.Pending
        });
    }

    public async Task AcceptFriendRequestAsync(Guid requestId, Guid currentUserId)
    {
        try
        {
            var friendship = await _friendshipsRepository.GetByIdAsync(requestId);

            if (friendship.AddresseeId != currentUserId)
                throw new UnauthorizedAccessException("You cannot accept this request");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new InvalidOperationException("Request is already accepted");

            friendship.Status = FriendshipStatus.Accepted;
            await _friendshipsRepository.UpdateAsync(friendship);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("Friendship not found! You cant accept request!", ex);
        }
    }

    public async Task RejectFriendRequestAsync(Guid requestId, Guid currentUserId)
    {
        try
        {
            var friendship = await _friendshipsRepository.GetByIdAsync(requestId);

            if (friendship.AddresseeId != currentUserId)
                throw new UnauthorizedAccessException("You cannot accept this request");

            if (friendship.Status != FriendshipStatus.Pending && friendship.Status != FriendshipStatus.Rejected)
                throw new InvalidOperationException($"Request is already {friendship.Status}");

            friendship.Status = FriendshipStatus.Rejected;
            await _friendshipsRepository.UpdateAsync(friendship);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("Friendship not found! You cant reject request!", ex);
        }
    }

    public async Task<List<Friendship>> GetIncomingRequestsAsync(Guid userId)
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