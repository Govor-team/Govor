using Govor.Application.Exceptions;
using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services;

public class FriendsService : IFriendsService
{
    private IUsersRepository _usersRepository;
    private IFriendshipsRepository _friendshipsRepository;
    
    public FriendsService(IUsersRepository usersRepository, IFriendshipsRepository relationshipsRepository)
    {
        _usersRepository = usersRepository;
        _friendshipsRepository = relationshipsRepository;
    }
    
    public async Task<List<User>> SearchUsersAsync(string searchTerm, Guid currentId)
    {
        var all = await _usersRepository.SearchPotentialFriendsAsync(currentId, searchTerm);

        try
        {
            var friends = await _friendshipsRepository.FindByUserIdAsync(currentId);

            friends = friends.Where(f => f.Status == FriendshipStatus.Accepted).ToList();

            return all
                .Where(u => u.Id != currentId && !friends.Select(f => f.RequesterId).Contains(u.Id))
                .ToList();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            return all.Where(u => u.Id != currentId).ToList();
        }
        catch (Exception ex)
        {
            throw new SearchUsersException($"When we try find friends by pattern {searchTerm} something wrong", ex);
        }
    }

    public async Task SendFriendRequestAsync(Guid fromUserId, Guid toUserId)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("Cannot send a request to self user");
        
        if (_friendshipsRepository.Exist(fromUserId, toUserId))
            throw new RequestAlreadySentException(fromUserId, toUserId);
        
        await _friendshipsRepository.AddAsync(new Friendship()
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
        catch (NotFoundByKeyException<Guid> e)
        {
            throw new InvalidOperationException("Friendship not found! You cant accept request!", e);
        }
    }

    public async Task<List<User>> GetFriendsAsync(Guid userId)
    {
        try
        {
            var user = await _usersRepository.FindByIdAsync(userId);

            var sent = user.SentFriendRequests
                .Where(f => f.Status == FriendshipStatus.Accepted)
                .Select(f => f.Addressee);

            var received = user.ReceivedFriendRequests
                .Where(f => f.Status == FriendshipStatus.Accepted)
                .Select(f => f.Requester);

            return sent.Concat(received).ToList();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("User not exist", ex);
        }
    }

    public async Task<List<Friendship>> GetIncomingRequestsAsync(Guid userId)
    {
        try
        {
            var user = await _usersRepository.FindByIdAsync(userId);
            return user.ReceivedFriendRequests;
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("User not exist", ex);
        }
    }
}

