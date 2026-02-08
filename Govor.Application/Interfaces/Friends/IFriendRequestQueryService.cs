using Govor.Core.Models;

namespace Govor.Application.Interfaces.Friends;


public interface IFriendRequestQueryService
{
    Task<List<Friendship>> GetIncomingAsync(Guid userId);
    Task<List<Friendship>> GetResponsesAsync(Guid userId);
}
