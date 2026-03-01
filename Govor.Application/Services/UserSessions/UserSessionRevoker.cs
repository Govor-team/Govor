using Govor.Application.Interfaces.UserSession;
using Govor.Core.Repositories.PushTokens;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.UserSessions;

public class UserSessionRevoker : IUserSessionRevoker
{
    private readonly IUserSessionsRepository _sessionsRepository;
    private readonly IPushTokenRepository _pushTokenRepository;
    private readonly ILogger<UserSessionRevoker> _logger;

    public UserSessionRevoker(
        IUserSessionsRepository sessionsRepository,
        IPushTokenRepository pushTokenRepository,
        ILogger<UserSessionRevoker> logger)
    {
        _sessionsRepository = sessionsRepository;
        _pushTokenRepository = pushTokenRepository;
        _logger = logger;
    }

    public async Task CloseSessionByIdAsync(Guid sessionId, Guid userId)
    {
        try
        {
            var session = await _sessionsRepository.GetByIdAsync(sessionId);

            if (session.UserId != userId)
            {
                _logger.LogWarning("User {userId} does not belong to this session {sessionId}", userId, sessionId);
                throw new UnauthorizedAccessException($"User {userId} does not belong to this session {sessionId}");
            }
            
            if(session.IsRevoked) return;
            
            session.IsRevoked = true;
            await _sessionsRepository.UpdateAsync(session);
            
            await _pushTokenRepository.DeactivateTokenBySessionAsync(sessionId); // pushes deactivate 
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning("User {userId} tried close unreal session!", userId);
            throw new NotFoundException("Session not found", ex);
        }
    }

    public async Task CloseAllSessionsAsync(Guid userId)
    {
        try
        {
            var sessions = await _sessionsRepository.GetByUserIdAsync(userId);

            foreach (var session in sessions)
            {
                if(session.IsRevoked) continue;
                
                session.IsRevoked = true;
                await _sessionsRepository.UpdateAsync(session);
                
                await _pushTokenRepository.DeactivateTokenBySessionAsync(session.Id); // pushes deactivate 
            }
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning("User {userId} has not sessions", userId);
            throw new NotFoundException("Session not found", ex);
        }
    }
}