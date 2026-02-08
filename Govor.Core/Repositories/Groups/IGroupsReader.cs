using Govor.Core.Models;

namespace Govor.Core.Repositories.Groups;

public interface IGroupsReader
{
    public Task<List<ChatGroup>> GetAllAsync();
    public Task<ChatGroup> GetByIdAsync(Guid id);
    public Task<List<ChatGroup>> SearchByNameAsync(string name);
    public Task<List<ChatGroup>> GetByAdminIdAsync(Guid userId);
    public Task<List<ChatGroup>> GetByUserIdAsync(Guid userId);
    public Task<bool> IsUserMemberOfGroupAsync(Guid userId, Guid groupId);
}