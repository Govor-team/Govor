using Govor.Core.Models;

namespace Govor.Core.Repositories.UserSessionsRepository;

public interface IUserSessionsRepository : IUserSessionsReader, IUserSessionsWriter, IUserSessionsExist
{
    
}