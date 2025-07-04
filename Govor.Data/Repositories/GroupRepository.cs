using Govor.Core.Models;
using Govor.Core.Repositories.Groups;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq; // Required for AnyAsync

namespace Govor.Data.Repositories;

public class GroupRepository : IGroupsRepository
{
    private readonly GovorDbContext _context;

    public GroupRepository(GovorDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid groupId)
    {
        return await _context.ChatGroups.AnyAsync(g => g.Id == groupId);
    }

    public async Task<bool> IsUserMemberOfGroupAsync(Guid userId, Guid groupId)
    {
        // Assuming ChatGroup has a navigation property 'Members' or a 'GroupMemberships' table
        // Adjust based on your actual data model for group membership
        var group = await _context.ChatGroups
            .Include(g => g.Memberships) // Assuming GroupMembership links User and Group
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return false; // Group doesn't exist
        }
        // Check if any membership for this group includes the user
        return group.Memberships.Any(m => m.UserId == userId);
    }

    // Implement other IGroupsReader methods if any were defined beyond ExistsAsync and IsUserMemberOfGroupAsync
    // For example:
    // public async Task<ChatGroup?> GetByIdAsync(Guid groupId)
    // {
    //     return await _context.ChatGroups.FindAsync(groupId);
    // }

    // public async Task<List<User>> GetGroupMembersAsync(Guid groupId)
    // {
    //     var group = await _context.ChatGroups
    //         .Include(g => g.Memberships)
    //         .ThenInclude(gm => gm.User)
    //         .FirstOrDefaultAsync(g => g.Id == groupId);
    //     if (group == null) return new List<User>();
    //     return group.Memberships.Select(gm => gm.User).ToList();
    // }
}