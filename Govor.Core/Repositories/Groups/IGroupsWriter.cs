using System.Text.RegularExpressions;

namespace Govor.Core.Repositories.Groups;

public interface IGroupsWriter
{
   Task Add(Group group);
   Task Update(Group group);
   Task Remove(Guid groupId);
}