using Govor.Core.Models;

namespace Govor.API.Services;

public interface IGroupService
{
    ChatGroup GetGroupByInvite(string code);
}