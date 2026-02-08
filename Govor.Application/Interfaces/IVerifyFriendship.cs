namespace Govor.Application.Interfaces;

public interface IVerifyFriendship
{
    Task VerifyAsync(Guid targetUserId, Guid friendUserId);
    Task<bool> TryVerifyAsync(Guid targetUserId, Guid friendUserId);
}