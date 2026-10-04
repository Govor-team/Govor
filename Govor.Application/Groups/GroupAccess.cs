using Govor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Groups;

public static class GroupAccess
{
    public static Task<bool> IsGroupAdministratorAsync(this GovorDbContext context, Guid groupId, Guid userId) =>
        context.ChatGroups.AnyAsync(g => g.Id == groupId &&
            context.GroupMemberships.Any(m => m.GroupId == groupId && m.UserId == userId && !m.IsBanned) &&
            (g.OwnerUserId == userId || context.GroupAdmins.Any(a => a.GroupId == groupId && a.UserId == userId)));
}
