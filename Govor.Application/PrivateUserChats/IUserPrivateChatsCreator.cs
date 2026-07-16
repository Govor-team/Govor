using Govor.Domain.Models;

namespace Govor.Application.PrivateUserChats;

public interface IUserPrivateChatsCreator
{
    Task<PrivateChat> CreateAsync(Guid userIdA, Guid userIdB);
}