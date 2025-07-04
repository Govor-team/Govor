using Govor.Core.Models;

namespace Govor.Core.Repositories.Groups;

public interface IGroupsExist
{
    public bool Exists(Guid groupId);
    public bool Exists(ChatGroup chatGroup);
}