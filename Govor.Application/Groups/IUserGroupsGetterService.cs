using Govor.Domain.Models;

namespace Govor.Application.Groups;

public interface IUserGroupsGetterService
{
    Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId);
}