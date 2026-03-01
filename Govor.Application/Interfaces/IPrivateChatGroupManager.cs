using Govor.Core.Models;

namespace Govor.Application.Interfaces;

public interface IPrivateChatGroupManager
{
    Task AddUsersToPrivateChatGroupAsync(PrivateChat privateChat);
    Task RemoveUsersFromPrivateChatGroupAsync(PrivateChat privateChat);
}