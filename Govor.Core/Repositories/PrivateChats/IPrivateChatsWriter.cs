using Govor.Core.Models;

namespace Govor.Core.Repositories.PrivateChats;

public interface IPrivateChatsWriter
{
    Task AddAsync(PrivateChat chat);
    Task UpdateAsync(PrivateChat chat);
    Task RemoveAsync(Guid chatId);
}