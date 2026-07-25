using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;
using SmartRes;

namespace Govor.Application.Groups;

public interface IGroupService
{
    Task<ChatGroup> GetGroupByIdAsync(Guid groupId);
    Task<ChatGroup> CreateGroupAsync(string name, Guid creatorId, IEnumerable<Guid> initialMemberIds);
    Task<Result<Unit, Error>> AddUserToGroupByInvitationAsync(Guid userId, string invitationCode);
    Task<Result<Unit, Error>> RemoveUserFromGroupAsync(Guid groupId, Guid userId, Guid removedByUserId);
    Task<Result<Unit, Error>> DeleteGroupAsync(Guid groupId, Guid userId); 
    Task<List<User>> GetGroupMembersAsync(Guid groupId); 
    Task<List<ChatGroup>>GetUserGroupsAsync(Guid userId); 
    ChatGroup GetGroupByInviteCode(string code);
}