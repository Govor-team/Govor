using Govor.Core.Models;

namespace Govor.Application.Interfaces;

public interface IUserGroupsGetterService
{
    Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId);
}