using Govor.Core.Models;

namespace Govor.Application.Interfaces;

public interface IUserPrivateChatsGetterService
{ 
    Task<List<PrivateChat>> GetUserChatsAsync(Guid userId);
}