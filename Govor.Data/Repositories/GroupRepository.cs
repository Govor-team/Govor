using System.Text.RegularExpressions;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories.Groups;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class GroupRepository : IGroupsRepository
{
    private GovorDbContext _context;
    private IObjectValidator<ChatGroup> _validator;
    
    public GroupRepository(GovorDbContext context, IObjectValidator<ChatGroup> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<List<ChatGroup>> GetAllAsync()
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Include(g => g.Admins)
            .Include(g => g.Members)
            .Include(g => g.InviteCodes)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public async Task<ChatGroup> GetByIdAsync(Guid id)
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Include(g => g.Admins)
            .Include(g => g.Members)
            .Include(g => g.InviteCodes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Id == id)
                ?? throw new NotFoundByKeyException<Guid>(id, "Group with given id doesn't exist");
    }

    public async Task<List<ChatGroup>> SearchByNameAsync(string name)
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Include(g => g.Admins)
            .Include(g => g.Members)
            .Include(g => g.InviteCodes)
            .AsSplitQuery()
            .Where(g => g.Name.ToLower().Contains(name.ToLower()))
            .Take(20)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<string>(name, "Group with name doesn't exist"));
    }
    
    public async Task<List<ChatGroup>> GetByAdminIdAsync(Guid userId)
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Include(g => g.Admins)
            .Include(g => g.Members)
            .Include(g => g.InviteCodes)
            .AsSplitQuery()
            .Where(g => g.Admins.Any(a => a.UserId == userId))
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(userId, "Admin with given id doesn't exist"));
    }

    public async Task<List<ChatGroup>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Include(g => g.Admins)
            .Include(g => g.Members)
            .Include(g => g.InviteCodes)
            .AsSplitQuery()
            .Where(g => g.Members.Any(a => a.UserId == userId))
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(userId, "Member with given id doesn't exist"));
    }

    public async Task AddAsync(ChatGroup group)
    {
        try
        {
            _validator.Validate(group);

            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<ChatGroup> ex)
        {
            throw new AdditionException("Group with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add group", ex);
        }
    }

    public async Task UpdateAsync(ChatGroup group)
    {
        try
        {
            _validator.Validate(group);

            var rowsAffected = await _context.ChatGroups
                .Where(u => u.Id == group.Id)
                .ExecuteUpdateAsync(g => g
                    .SetProperty(g => g.Name, group.Name)
                    .SetProperty(g => g.Description, group.Description)
                    .SetProperty(g => g.ImageId, group.ImageId)
                    .SetProperty(g => g.IsChannel, group.IsChannel)
                    .SetProperty(g => g.IsPrivate, group.IsPrivate)
                    .SetProperty(g => g.InviteCodes, group.InviteCodes)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found group by given id {group.Id}");
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the group {group.Id}", ex);
        }
    }

    public async Task RemoveAsync(Guid groupId)
    {
        try
        {
            var result = await GetByIdAsync(groupId);
            
            _context.ChatGroups.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<User> ex)
        {
            throw new RemoveException("User with given data invalid", ex);
        }
    }
    
    public bool Exists(Guid groupId)
    {
        return _context.ChatGroups.Any(g => g.Id == groupId);
    }

    public bool Exists(ChatGroup chatGroup)
    {
        return _context.ChatGroups.Any(g => g.Id == chatGroup.Id &&
                                            g.IsChannel == chatGroup.IsChannel &&
                                            g.Description == chatGroup.Description &&
                                            g.IsPrivate == chatGroup.IsPrivate &&
                                            g.Name == chatGroup.Name &&
                                            g.ImageId == chatGroup.ImageId);
    }
    
    public async Task<bool> IsUserMemberOfGroupAsync(Guid userId, Guid groupId)
    {
        return await _context.ChatGroups
            .AsNoTracking()
            .Include(g => g.Members)
            .AnyAsync(g => g.Members.Any(a => a.UserId == userId && a.GroupId == groupId));
    }
}
