using Govor.Core.Models;

namespace Govor.Core.Services;

public interface IGroupService
{
    ChatGroup GetGroupByInvite(string code);
}