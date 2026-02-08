using Govor.Core.Models;

namespace Govor.Core.Repositories.PrivateChats;

public interface IPrivateChatsReader
{
    Task<List<PrivateChat>> GetAllAsync();
    Task<PrivateChat> GetByIdAsync(Guid id);
    Task<PrivateChat> GetByMembersAsync(Guid memberAId, Guid memberBId);
    Task<List<PrivateChat>> GetAllOfUser(Guid userId);
}