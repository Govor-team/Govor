using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Friendships;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services.Friends;

public class FriendshipService : IFriendshipService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IFriendshipsRepository _friendshipsRepository;

    public FriendshipService(IUsersRepository usersRepository, IFriendshipsRepository friendshipsRepository)
    {
        _usersRepository = usersRepository;
        _friendshipsRepository = friendshipsRepository;
    }

    public async Task<List<User>> SearchUsersAsync(string query, Guid currentId)
    {
        List<User> all = new List<User>();
        
        try
        {
            all = await _usersRepository.SearchPotentialFriendsAsync(currentId, query);

            return all
                .Where(u => u.Id != currentId)
                .ToList();
        }
        catch (NotFoundByKeyException<(string, Guid)> ex)
        {
            throw new SearchUsersException(
                $"Users with given query: \"{query}\" for user with id {currentId} was not found", ex);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            return all.Where(u => u.Id != currentId).ToList();
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"When we try find friends by pattern {query} something wrong", ex);
        }
    }

    public async Task<List<User>> GetFriendsAsync(Guid userId)
    {
        try
        {
            var friendships = await _friendshipsRepository.FindByUserIdAsync(userId);

            return friendships
                .Where(f => f.Status == FriendshipStatus.Accepted)
                .Select(f => f.RequesterId == userId ? f.Addressee : f.Requester)
                .ToList();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new InvalidOperationException("User not found", ex);
        }
    }
}