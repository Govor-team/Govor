using Govor.Domain;
using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Groups;

public class UserGroupsGetterService : IUserGroupsGetterService
{
    private readonly GovorDbContext _context;

    public UserGroupsGetterService(GovorDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId)
    {
        return await _context.GroupMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.ChatGroup) 
            .ToListAsync();
    }

}