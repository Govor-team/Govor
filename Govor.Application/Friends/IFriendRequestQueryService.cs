using Govor.Domain.Models;

namespace Govor.Application.Friends;


public interface IFriendRequestQueryService
{
    Task<List<Friendship>> GetIncomingAsync(Guid userId);
    Task<List<Friendship>> GetResponsesAsync(Guid userId);
}
