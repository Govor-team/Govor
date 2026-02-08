using Govor.Core.Models;

namespace Govor.Application.Interfaces;

public interface IUserGroupsService
{
    Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId);
}