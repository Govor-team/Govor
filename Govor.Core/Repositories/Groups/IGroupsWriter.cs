using Govor.Core.Models;

namespace Govor.Core.Repositories.Groups;

public interface IGroupsWriter
{
   Task AddAsync(ChatGroup group);
   Task UpdateAsync(ChatGroup group);
   Task RemoveAsync(Guid groupId);
}