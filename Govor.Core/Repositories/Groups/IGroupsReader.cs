using System.Text.RegularExpressions;

namespace Govor.Core.Repositories.Groups;

public interface IGroupsReader
{
    public Task<List<Group>> GetAllAsync();
    public Task<Group> GetByIdAsync(Guid id);
    public Task<List<Group>> FindByNameAsync(string name);
    public Task<List<Group>> GetByAdminIdAsync(Guid adminId);
    public Task<List<Group>> GetByUserIdAsync(Guid adminId);
    public bool IsUserMemberOfGroupAsync(Guid userId, Guid groupId);
}