namespace Govor.Core.Repositories.Friendships;

public interface IFriendshipsExist
{
    bool Exist(Guid requesterId, Guid addresseeId);
    bool IsPossibilityOfCreatingFriendship(Guid requesterId, Guid addresseeId);
}