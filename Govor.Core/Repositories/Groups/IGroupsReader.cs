namespace Govor.Core.Repositories.Groups;

public interface IGroupsReader // Changed to interface
{
    Task<bool> ExistsAsync(Guid groupId);
    Task<bool> IsUserMemberOfGroupAsync(Guid userId, Guid groupId);
    // Potentially other read methods like:
    // Task<ChatGroup?> GetByIdAsync(Guid groupId);
    // Task<List<User>> GetGroupMembersAsync(Guid groupId);
}