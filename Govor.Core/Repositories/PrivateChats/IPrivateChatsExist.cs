namespace Govor.Core.Repositories.PrivateChats;

public interface IPrivateChatsExist
{
    bool Exist(Guid chatId);
    bool Exist(Guid userAId, Guid userBId);
}