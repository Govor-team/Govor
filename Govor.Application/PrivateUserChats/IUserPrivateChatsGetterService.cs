using Govor.Domain.Common;
using Govor.Domain.Models;
using SmartRes;

namespace Govor.Application.PrivateUserChats;

public interface IUserPrivateChatsGetterService
{ 
    Task<List<PrivateChat>> GetUserChatsAsync(Guid userId);
    Task<Result<PrivateChat, Error>> GetPrivateChatAsync(Guid chatId);
    Task<bool> ExistChatAsync(Guid userIdA, Guid userIdB);
}