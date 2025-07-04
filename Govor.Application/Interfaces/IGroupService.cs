using Govor.Application.Interfaces.Messages;
using Govor.Core.Models;

namespace Govor.API.Services;

public interface IGroupService : IMessageSendingService, IMessageManagementService
{
    ChatGroup GetGroupByInvite(string code);
}