using Govor.Domain.Common;
using Govor.Domain.Models;
using SmartRes;

namespace Govor.Application.Groups;

public record GroupSummary(ChatGroup Group, int MemberCount, GroupRole? MyRole, bool IsRequiredChannel, bool CanLeave);
public record GroupMember(Guid UserId, string Username, GroupRole Role, bool IsBanned, DateTime MemberSince);

public interface IGroupManagementService
{
    Task<Result<GroupSummary, Error>> CreateAsync(Guid actorId, string name, string description, bool isPrivate, bool isChannel);
    Task<Result<GroupSummary, Error>> GetAsync(Guid actorId, Guid groupId);
    Task<List<GroupSummary>> SearchAsync(Guid actorId, string query, int skip = 0, int take = 50);
    Task<List<GroupSummary>> MineAsync(Guid actorId, int skip = 0, int take = 50);
    Task<Result<GroupSummary, Error>> UpdateAsync(Guid actorId, Guid groupId, string name, string description, bool isPrivate);
    Task<Result<GroupSummary, Error>> JoinPublicAsync(Guid actorId, Guid groupId);
    Task<Result<GroupSummary, Error>> JoinByInvitationAsync(Guid actorId, string code);
    Task<Result<GroupSummary, Error>> PreviewInvitationAsync(Guid actorId, string code);
    Task<Result<Unit, Error>> LeaveAsync(Guid actorId, Guid groupId);
    Task<Result<List<GroupMember>, Error>> MembersAsync(Guid actorId, Guid groupId, bool bannedOnly, int skip, int take);
    Task<Result<Unit, Error>> SetAdministratorAsync(Guid actorId, Guid groupId, Guid targetId, bool isAdmin);
    Task<Result<Unit, Error>> SetBanAsync(Guid actorId, Guid groupId, Guid targetId, bool banned);
    Task<Result<Unit, Error>> TransferOwnershipAsync(Guid actorId, Guid groupId, Guid targetId);
    Task<Result<GroupInvitation, Error>> CreateInvitationAsync(Guid actorId, Guid groupId, int days, int maxParticipants, string description);
    Task<Result<List<GroupInvitation>, Error>> InvitationsAsync(Guid actorId, Guid groupId);
    Task<Result<Unit, Error>> RevokeInvitationAsync(Guid actorId, Guid groupId, Guid invitationId);
    Task<Result<ServerCommunitySettings, Error>> GetRequiredChannelAsync(Guid actorId);
    Task<Result<ServerCommunitySettings, Error>> SetRequiredChannelAsync(Guid actorId, Guid? channelId, bool allowLeave);
}
