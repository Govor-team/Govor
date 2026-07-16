namespace Govor.Application.Friends;

public interface IVerifyFriendship
{
    Task VerifyAsync(Guid targetUserId, Guid friendUserId);
    Task<bool> TryVerifyAsync(Guid targetUserId, Guid friendUserId);
}