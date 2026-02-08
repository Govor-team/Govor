using Govor.Core.Repositories.Groups;

namespace Govor.Core.Repositories.PrivateChats;

public interface IPrivateChatsRepository : IPrivateChatsReader, IPrivateChatsWriter, IPrivateChatsExist
{
    
}