using Govor.Application.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.PrivateChats;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services;

public class UserPrivateChatsGetter : IUserPrivateChatsGetterService
{
    private readonly IPrivateChatsRepository _groupRep;
    
    public UserPrivateChatsGetter(IPrivateChatsRepository groupsRepository)
    {
        _groupRep = groupsRepository;
    }
    
    public async Task<List<PrivateChat>> GetUserChatsAsync(Guid userId)
    {
        try
        {
            return await _groupRep.GetAllOfUser(userId);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            return [];
        }
    }
}