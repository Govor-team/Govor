using Govor.Core.Models;

namespace Govor.Core.Repositories.Groups;

public interface IGroupsExist
{
    public bool Exist(Guid groupId);
    public bool Exist(ChatGroup chatGroup);
}