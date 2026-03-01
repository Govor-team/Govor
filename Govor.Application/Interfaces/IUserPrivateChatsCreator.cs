using Govor.Core.Models;

namespace Govor.Application.Interfaces;

public interface IUserPrivateChatsCreator
{
    Task<PrivateChat> CreateAsync(Guid userIdA, Guid userIdB);
}