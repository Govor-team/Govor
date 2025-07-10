namespace Govor.Application.Interfaces.Friends;

public interface IFriendRequestCommandService
{
    Task SendAsync(Guid fromUserId, Guid toUserId);
    Task AcceptAsync(Guid requestId, Guid currentUserId);
    Task RejectAsync(Guid requestId, Guid currentUserId);
}
