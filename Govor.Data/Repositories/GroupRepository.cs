using System.Text.RegularExpressions;
using Govor.Core.Models;
using Govor.Core.Repositories.Groups;

namespace Govor.Data.Repositories;

public class GroupRepository : IGroupsRepository
{
    public Task<List<Group>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Group> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Group>> FindByNameAsync(string name)
    {
        throw new NotImplementedException();
    }
    
    public Task<List<Group>> GetByAdminIdAsync(Guid adminId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Group>> GetByUserIdAsync(Guid adminId)
    {
        throw new NotImplementedException();
    }

    public Task Add(Group group)
    {
        throw new NotImplementedException();
    }

    public Task Update(Group group)
    {
        throw new NotImplementedException();
    }

    public Task Remove(Guid groupId)
    {
        throw new NotImplementedException();
    }
    public bool Exists(Guid groupId)
    {
        throw new NotImplementedException();
    }

    public bool Exists(ChatGroup chatGroup)
    {
        throw new NotImplementedException();
    }
    
    public bool IsUserMemberOfGroupAsync(Guid userId, Guid groupId)
    {
        throw new NotImplementedException();
    }
}
