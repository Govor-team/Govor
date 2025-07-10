using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services.Friends;

public class FriendRequestCommandService : IFriendRequestCommandService
{
    private readonly IFriendshipsRepository _friendshipsRepository;

    public FriendRequestCommandService(IFriendshipsRepository friendshipsRepository)
    {
        _friendshipsRepository = friendshipsRepository;
    }
    
    public async Task SendAsync(Guid fromUserId, Guid toUserId)
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

    public async Task AcceptAsync(Guid requestId, Guid currentUserId)
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

    public async Task RejectAsync(Guid requestId, Guid currentUserId)
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
}