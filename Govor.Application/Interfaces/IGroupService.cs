using Govor.Core.Models; // For ChatGroup and User
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Govor.Application.Interfaces; // Corrected namespace

public interface IGroupService
{
    Task<ChatGroup?> GetGroupByIdAsync(Guid groupId); // Added
    Task<ChatGroup> CreateGroupAsync(string name, Guid creatorId, IEnumerable<Guid> initialMemberIds); // Added
    Task<Result> AddUserToGroupAsync(Guid groupId, Guid userId, Guid addedByUserId); // Added
    Task<Result> RemoveUserFromGroupAsync(Guid groupId, Guid userId, Guid removedByUserId); // Added
    Task<Result> DeleteGroupAsync(Guid groupId, Guid userId); // Added
    Task<List<User>> GetGroupMembersAsync(Guid groupId); // Added
    Task<List<ChatGroup>>GetUserGroupsAsync(Guid userId); // Added to support ChatsHub group joining

    // Existing method, assuming it's still relevant for group operations (e.g., joining via invite link)
    // Consider if this should return Task<ChatGroup?> or throw if not found.
    ChatGroup? GetGroupByInviteCode(string code); // Renamed parameter for clarity, made nullable
}
