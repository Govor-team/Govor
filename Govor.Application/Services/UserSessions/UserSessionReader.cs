using Govor.Application.Interfaces.UserSession;
using Govor.Core.Models;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.UserSessions;

public class UserSessionReader : IUserSessionReader
{
    private readonly IUserSessionsRepository _repository;
    private readonly ILogger<UserSessionReader> _logger;

    public UserSessionReader(IUserSessionsRepository repository, ILogger<UserSessionReader> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<List<UserSession>> GetAllSessionsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation($"Getting all sessions for user {userId}");
            var sessions = await _repository.GetByUserIdAsync(userId);
            
            return sessions.Where(f => !f.IsRevoked).ToList() ?? [];
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning("The user has no sessions.");
            return [];
        }
    }
}