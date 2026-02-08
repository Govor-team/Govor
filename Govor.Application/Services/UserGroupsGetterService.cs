using Govor.Application.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Groups;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services;

public class UserGroupsGetterService : IUserGroupsGetterService
{
    private readonly IGroupsRepository _groupRep;
    
    public UserGroupsGetterService(IGroupsRepository groupsRepository)
    {
        _groupRep = groupsRepository;
    }
    
    public async Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId)
    {
        try
        {
            return await _groupRep.GetByUserIdAsync(userId);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            return new List<ChatGroup>();
        }
    }
}