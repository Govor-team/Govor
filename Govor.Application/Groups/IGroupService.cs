using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;

namespace Govor.Application.Groups;

public interface IGroupService
{
    Task<ChatGroup> GetGroupByIdAsync(Guid groupId);
    Task<ChatGroup> CreateGroupAsync(string name, Guid creatorId, IEnumerable<Guid> initialMemberIds);
    Task<Result> AddUserToGroupByInvitationAsync(Guid userId, string invitationCode);
    Task<Result> RemoveUserFromGroupAsync(Guid groupId, Guid userId, Guid removedByUserId);
    Task<Result> DeleteGroupAsync(Guid groupId, Guid userId); 
    Task<List<User>> GetGroupMembersAsync(Guid groupId); 
    Task<List<ChatGroup>>GetUserGroupsAsync(Guid userId); 
    ChatGroup GetGroupByInviteCode(string code);
}