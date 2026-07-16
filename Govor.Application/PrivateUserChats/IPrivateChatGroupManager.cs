using Govor.Domain.Models;

namespace Govor.Application.PrivateUserChats;

public interface IPrivateChatGroupManager
{
    Task AddUsersToPrivateChatGroupAsync(PrivateChat privateChat);
    Task RemoveUsersFromPrivateChatGroupAsync(PrivateChat privateChat);
}