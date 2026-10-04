using Govor.Application.Infrastructure.Common;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using SmartRes;

namespace Govor.Application.Groups;

public class GroupManagementService(GovorDbContext context, INowDateTimeProvider clock) : IGroupManagementService
{
    public async Task<Result<GroupSummary, Error>> CreateAsync(Guid actorId, string name, string description, bool isPrivate, bool isChannel)
    {
        if (!ValidText(name, description)) return Result.Failure<GroupSummary>(Invalid("Invalid group profile."));
        if (!await context.Users.AnyAsync(u => u.Id == actorId)) return Result.Failure<GroupSummary>(Denied());
        var group = new ChatGroup
        {
            Id = Guid.NewGuid(), Name = name.Trim(), Description = description.Trim(), IsPrivate = isPrivate,
            IsChannel = isChannel, OwnerUserId = actorId, CreatedAt = clock.Now
        };
        group.Members.Add(new GroupMembership { Id = Guid.NewGuid(), GroupId = group.Id, UserId = actorId, MemberSince = clock.Now });
        context.ChatGroups.Add(group);
        await context.SaveChangesAsync();
        return await SummaryAsync(group, actorId);
    }

    public async Task<Result<GroupSummary, Error>> GetAsync(Guid actorId, Guid groupId)
    {
        var group = await context.ChatGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
        if (group is null || await IsBannedAsync(groupId, actorId) ||
            (group.IsPrivate && !await IsMemberAsync(groupId, actorId))) return Result.Failure<GroupSummary>(Missing());
        return await SummaryAsync(group, actorId);
    }

    public async Task<List<GroupSummary>> SearchAsync(Guid actorId, string query, int skip = 0, int take = 50)
    {
        query = (query ?? "").Trim().ToLowerInvariant();
        var groups = await context.ChatGroups.AsNoTracking().Where(g => !g.IsPrivate &&
            !context.GroupMemberships.Any(m => m.GroupId == g.Id && m.UserId == actorId && m.IsBanned) &&
            (query == "" || g.Name.ToLower().Contains(query) || g.Description.ToLower().Contains(query)))
            .OrderBy(g => g.Name).ThenBy(g => g.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        var result = new List<GroupSummary>();
        foreach (var group in groups) result.Add(await SummaryAsync(group, actorId));
        return result;
    }

    public async Task<List<GroupSummary>> MineAsync(Guid actorId, int skip = 0, int take = 50)
    {
        var groups = await context.ChatGroups.AsNoTracking().Where(g => context.GroupMemberships
            .Any(m => m.GroupId == g.Id && m.UserId == actorId && !m.IsBanned))
            .OrderBy(g => g.Name).ThenBy(g => g.Id).Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        var result = new List<GroupSummary>();
        foreach (var group in groups) result.Add(await SummaryAsync(group, actorId));
        return result;
    }

    public async Task<Result<GroupSummary, Error>> UpdateAsync(Guid actorId, Guid groupId, string name, string description, bool isPrivate)
    {
        if (!ValidText(name, description)) return Result.Failure<GroupSummary>(Invalid("Invalid group profile."));
        var changed = await MutateAsync(groupId, async group =>
        {
            if (!await IsOwnerAsync(group, actorId)) return Result.Failure(Denied());
            await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s =>
                s.SetProperty(g => g.Name, name.Trim()).SetProperty(g => g.Description, description.Trim())
                    .SetProperty(g => g.IsPrivate, isPrivate));
            return Result.Success();
        });
        return changed.IsFailure ? Result.Failure<GroupSummary>(changed.Error) : await GetAsync(actorId, groupId);
    }

    public Task<Result<GroupSummary, Error>> JoinPublicAsync(Guid actorId, Guid groupId) => JoinAsync(actorId, groupId, null);

    public async Task<Result<GroupSummary, Error>> JoinByInvitationAsync(Guid actorId, string code)
    {
        var groupId = await context.GroupInvitations.Where(i => i.InvitationCode == code)
            .Select(i => (Guid?)i.GroupId).FirstOrDefaultAsync();
        return groupId.HasValue ? await JoinAsync(actorId, groupId.Value, code) : Result.Failure<GroupSummary>(Missing());
    }

    public async Task<Result<GroupSummary, Error>> PreviewInvitationAsync(Guid actorId, string code)
    {
        var invitation = await context.GroupInvitations.AsNoTracking().FirstOrDefaultAsync(i => i.InvitationCode == code);
        if (invitation is null || !Usable(invitation) || await IsBannedAsync(invitation.GroupId, actorId))
            return Result.Failure<GroupSummary>(Missing());
        var group = await context.ChatGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == invitation.GroupId);
        return group is null ? Result.Failure<GroupSummary>(Missing()) : Result.Success(await SummaryAsync(group, actorId));
    }

    private async Task<Result<GroupSummary, Error>> JoinAsync(Guid actorId, Guid groupId, string? code)
    {
        var joined = await MutateAsync(groupId, async group =>
        {
            if (!await context.Users.AnyAsync(u => u.Id == actorId)) return Result.Failure(Denied());
            var membership = await context.GroupMemberships.AsNoTracking()
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == actorId);
            if (membership?.IsBanned == true) return Result.Failure(Denied());
            GroupInvitation? invitation = null;
            if (code is not null)
            {
                invitation = await context.GroupInvitations.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.GroupId == groupId && i.InvitationCode == code);
                if (invitation is null || invitation.IsRevoked || invitation.EndDate <= clock.Now) return Result.Failure(Missing());
            }
            else if (group.IsPrivate) return Result.Failure(Denied());
            if (membership is not null) return Result.Success();
            if (invitation is not null && !Usable(invitation)) return Result.Failure(Invalid("Invitation has reached its usage limit."));
            var id = Guid.NewGuid();
            Guid? invitationId = invitation?.Id;
            var now = clock.Now;
            await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"GroupMemberships\" (\"Id\", \"GroupId\", \"UserId\", \"InvitationId\", \"IsBanned\", \"MemberSince\") VALUES ({id}, {groupId}, {actorId}, {invitationId}, {false}, {now})");
            if (invitation is not null)
                await context.GroupInvitations.Where(i => i.Id == invitation.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(i => i.UsedCount, i => i.UsedCount + 1));
            return Result.Success();
        });
        return joined.IsFailure ? Result.Failure<GroupSummary>(joined.Error) : await GetAsync(actorId, groupId);
    }

    public Task<Result<Unit, Error>> LeaveAsync(Guid actorId, Guid groupId) => MutateAsync(groupId, async group =>
    {
        if (!await IsMemberAsync(groupId, actorId)) return Result.Failure(Denied());
        if (group.OwnerUserId == actorId) return Result.Failure(Invalid("Transfer ownership before leaving."));
        if (await context.ServerCommunitySettings.AnyAsync(s => s.Id == 1 && s.RequiredChannelId == groupId && !s.AllowLeave))
            return Result.Failure(Denied("Leaving the required channel is disabled."));
        await context.GroupAdmins.Where(a => a.GroupId == groupId && a.UserId == actorId).ExecuteDeleteAsync();
        await context.GroupMemberships.Where(m => m.GroupId == groupId && m.UserId == actorId).ExecuteDeleteAsync();
        return Result.Success();
    });

    public async Task<Result<List<GroupMember>, Error>> MembersAsync(Guid actorId, Guid groupId, bool bannedOnly, int skip, int take)
    {
        if (!await IsMemberAsync(groupId, actorId) || (bannedOnly && !await context.IsGroupAdministratorAsync(groupId, actorId)))
            return Result.Failure<List<GroupMember>>(Denied());
        var group = await context.ChatGroups.AsNoTracking().FirstAsync(g => g.Id == groupId);
        var members = await (from m in context.GroupMemberships.AsNoTracking()
            join u in context.Users on m.UserId equals u.Id
            where m.GroupId == groupId && m.IsBanned == bannedOnly
            orderby m.MemberSince, m.UserId
            select new { m.UserId, u.Username, m.MemberSince, m.IsBanned,
                Admin = context.GroupAdmins.Any(a => a.GroupId == groupId && a.UserId == m.UserId) })
            .Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100)).ToListAsync();
        return members.Select(m => new GroupMember(m.UserId, m.Username,
            group.OwnerUserId == m.UserId ? GroupRole.Owner : m.Admin ? GroupRole.Admin : GroupRole.Member,
            m.IsBanned, m.MemberSince)).ToList();
    }

    public Task<Result<Unit, Error>> SetAdministratorAsync(Guid actorId, Guid groupId, Guid targetId, bool isAdmin) =>
        MutateAsync(groupId, async group =>
        {
            if (!await IsOwnerAsync(group, actorId)) return Result.Failure(Denied());
            if (group.OwnerUserId == targetId || !await IsMemberAsync(groupId, targetId))
                return Result.Failure(Invalid("Choose an active member other than the owner."));
            if (isAdmin)
            {
                var id = Guid.NewGuid();
                await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"GroupAdmins\" (\"Id\", \"GroupId\", \"UserId\") VALUES ({id}, {groupId}, {targetId}) ON CONFLICT (\"GroupId\", \"UserId\") DO NOTHING");
            }
            else await context.GroupAdmins.Where(a => a.GroupId == groupId && a.UserId == targetId).ExecuteDeleteAsync();
            return Result.Success();
        });

    public Task<Result<Unit, Error>> SetBanAsync(Guid actorId, Guid groupId, Guid targetId, bool banned) => MutateAsync(groupId, async group =>
    {
        if (!await context.IsGroupAdministratorAsync(groupId, actorId)) return Result.Failure(Denied());
        if (targetId == actorId || group.OwnerUserId == targetId ||
            (group.OwnerUserId != actorId && await context.GroupAdmins.AnyAsync(a => a.GroupId == groupId && a.UserId == targetId)))
            return Result.Failure(Denied("Administrators cannot moderate the owner or another administrator."));
        if (!await context.GroupMemberships.AnyAsync(m => m.GroupId == groupId && m.UserId == targetId))
            return Result.Failure(Missing());
        await context.GroupMemberships.Where(m => m.GroupId == groupId && m.UserId == targetId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsBanned, banned));
        if (banned) await context.GroupAdmins.Where(a => a.GroupId == groupId && a.UserId == targetId).ExecuteDeleteAsync();
        return Result.Success();
    });

    public Task<Result<Unit, Error>> TransferOwnershipAsync(Guid actorId, Guid groupId, Guid targetId) => MutateAsync(groupId, async group =>
    {
        if (!await IsOwnerAsync(group, actorId)) return Result.Failure(Denied());
        if (targetId == actorId || !await IsMemberAsync(groupId, targetId)) return Result.Failure(Invalid("Choose another active member."));
        await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s => s.SetProperty(g => g.OwnerUserId, targetId));
        var id = Guid.NewGuid();
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"GroupAdmins\" (\"Id\", \"GroupId\", \"UserId\") VALUES ({id}, {groupId}, {actorId}) ON CONFLICT (\"GroupId\", \"UserId\") DO NOTHING");
        await context.GroupAdmins.Where(a => a.GroupId == groupId && a.UserId == targetId).ExecuteDeleteAsync();
        return Result.Success();
    });

    public async Task<Result<GroupInvitation, Error>> CreateInvitationAsync(Guid actorId, Guid groupId, int days, int maxParticipants, string description)
    {
        if (days is < 1 or > 365 || maxParticipants is < 0 or > 1000000 || description is null || description.Length > 500)
            return Result.Failure<GroupInvitation>(Invalid("Invalid invitation limits."));
        var invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(), GroupId = groupId, UserMakerId = actorId, InvitationCode = Guid.NewGuid().ToString("N"),
            Description = description.Trim(), CreatedAt = clock.Now, EndDate = clock.Now.AddDays(days), MaxParticipants = maxParticipants
        };
        var result = await MutateAsync(groupId, async group =>
        {
            if (!await context.IsGroupAdministratorAsync(groupId, actorId)) return Result.Failure(Denied());
            // A stable ID makes an execution-strategy retry safe after an ambiguous commit.
            if (!await context.GroupInvitations.AnyAsync(i => i.Id == invitation.Id))
                await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"GroupInvitations\" (\"Id\", \"GroupId\", \"UserMakerId\", \"InvitationCode\", \"Description\", \"CreatedAt\", \"EndDate\", \"MaxParticipants\", \"UsedCount\", \"IsRevoked\") VALUES ({invitation.Id}, {groupId}, {actorId}, {invitation.InvitationCode}, {invitation.Description}, {invitation.CreatedAt}, {invitation.EndDate}, {maxParticipants}, {0}, {false})");
            return Result.Success();
        });
        return result.IsFailure ? Result.Failure<GroupInvitation>(result.Error) : Result.Success(invitation);
    }

    public async Task<Result<List<GroupInvitation>, Error>> InvitationsAsync(Guid actorId, Guid groupId)
    {
        if (!await context.IsGroupAdministratorAsync(groupId, actorId)) return Result.Failure<List<GroupInvitation>>(Denied());
        return await context.GroupInvitations.AsNoTracking().Where(i => i.GroupId == groupId)
            .OrderByDescending(i => i.CreatedAt).ThenBy(i => i.Id).Take(100).ToListAsync();
    }

    public Task<Result<Unit, Error>> RevokeInvitationAsync(Guid actorId, Guid groupId, Guid invitationId) => MutateAsync(groupId, async group =>
    {
        if (!await context.IsGroupAdministratorAsync(groupId, actorId)) return Result.Failure(Denied());
        var count = await context.GroupInvitations.Where(i => i.Id == invitationId && i.GroupId == groupId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsRevoked, true));
        return count == 0 ? Result.Failure(Missing()) : Result.Success();
    });

    public async Task<Result<ServerCommunitySettings, Error>> GetRequiredChannelAsync(Guid actorId)
    {
        if (!await IsServerAdminAsync(actorId)) return Result.Failure<ServerCommunitySettings>(Denied());
        return await context.ServerCommunitySettings.AsNoTracking().SingleAsync(s => s.Id == 1);
    }

    public async Task<Result<ServerCommunitySettings, Error>> SetRequiredChannelAsync(Guid actorId, Guid? channelId, bool allowLeave)
    {
        if (channelId == Guid.Empty) return Result.Failure<ServerCommunitySettings>(Invalid("Use a valid channel ID or null."));
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await context.ServerCommunitySettings.Where(s => s.Id == 1)
                .ExecuteUpdateAsync(s => s.SetProperty(s => s.AllowLeave, s => s.AllowLeave));
            if (!await IsServerAdminAsync(actorId)) return Result.Failure<ServerCommunitySettings>(Denied());
            if (channelId.HasValue && !await context.ChatGroups.AnyAsync(g => g.Id == channelId && g.IsChannel && g.OwnerUserId != null))
                return Result.Failure<ServerCommunitySettings>(Invalid("Choose an existing channel with an owner."));
            await context.ServerCommunitySettings.Where(s => s.Id == 1).ExecuteUpdateAsync(s =>
                s.SetProperty(s => s.RequiredChannelId, channelId).SetProperty(s => s.AllowLeave, allowLeave));
            var settings = await context.ServerCommunitySettings.AsNoTracking().SingleAsync(s => s.Id == 1);
            await transaction.CommitAsync();
            return Result.Success(settings);
        });
    }

    // Every membership/role/invitation decision is serialized on its group row.
    private Task<Result<Unit, Error>> MutateAsync(Guid groupId, Func<ChatGroup, Task<Result<Unit, Error>>> mutation) =>
        context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var count = await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s => s.SetProperty(g => g.Name, g => g.Name));
            if (count == 0) return Result.Failure(Missing());
            var group = await context.ChatGroups.AsNoTracking().SingleAsync(g => g.Id == groupId);
            var result = await mutation(group);
            if (result.IsSuccess) await transaction.CommitAsync();
            return result;
        });

    private async Task<GroupSummary> SummaryAsync(ChatGroup group, Guid actorId)
    {
        var member = await IsMemberAsync(group.Id, actorId);
        GroupRole? role = member ? group.OwnerUserId == actorId ? GroupRole.Owner :
            await context.GroupAdmins.AnyAsync(a => a.GroupId == group.Id && a.UserId == actorId) ? GroupRole.Admin : GroupRole.Member : null;
        var settings = await context.ServerCommunitySettings.AsNoTracking().SingleAsync(s => s.Id == 1);
        var required = settings.RequiredChannelId == group.Id;
        return new GroupSummary(group, await context.GroupMemberships.CountAsync(m => m.GroupId == group.Id && !m.IsBanned),
            role, required, member && group.OwnerUserId != actorId && (!required || settings.AllowLeave));
    }

    private Task<bool> IsMemberAsync(Guid groupId, Guid userId) => context.GroupMemberships.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && !m.IsBanned);
    private Task<bool> IsBannedAsync(Guid groupId, Guid userId) => context.GroupMemberships.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsBanned);
    private async Task<bool> IsOwnerAsync(ChatGroup group, Guid userId) => group.OwnerUserId == userId && await IsMemberAsync(group.Id, userId);
    private Task<bool> IsServerAdminAsync(Guid userId) => context.Users.AnyAsync(u => u.Id == userId && u.Invite != null && u.Invite.IsAdmin);
    private bool Usable(GroupInvitation invitation) => !invitation.IsRevoked && invitation.EndDate > clock.Now &&
        (invitation.MaxParticipants == 0 || invitation.UsedCount < invitation.MaxParticipants);
    private static bool ValidText(string name, string description) => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100 && description is not null && description.Length <= 500;
    private static Error Invalid(string message) => Error.Validation("Group.Invalid", message);
    private static Error Missing() => Error.NotFound("Group.NotFound", "Group or invitation not found.");
    private static Error Denied(string message = "You do not have permission for this group operation.") => Error.Forbidden("Group.AccessDenied", message);
}
