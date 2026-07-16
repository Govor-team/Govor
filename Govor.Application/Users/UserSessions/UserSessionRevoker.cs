using Govor.Application.PushNotifications;
using Govor.Domain;
using Govor.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Users.UserSessions;

public class UserSessionRevoker : IUserSessionRevoker
{
    private readonly GovorDbContext _context;
    private readonly IPushTokenService _tokenService;
    private readonly ILogger<UserSessionRevoker> _logger;

    public UserSessionRevoker(
        GovorDbContext context,
        IPushTokenService tokenService,
        ILogger<UserSessionRevoker> logger)
    {
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    public async Task<Result> CloseSessionByIdAsync(Guid sessionId, Guid userId)
    {
        _logger.LogInformation("Attempting to close session {SessionId} for user {UserId}", sessionId, userId);

        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsRevoked);

            if (session is null)
            {
                _logger.LogWarning("Active session {SessionId} not found or doesn't belong to user {UserId}", sessionId, userId);
                return Result.Failure(new Error(
                    "UserSession.NotFoundOrUnauthorized", 
                    $"Active session {sessionId} for user {userId} was not found."));
            }

            session.IsRevoked = true;

            await _context.SaveChangesAsync();
            
             await _tokenService.DeactivateTokenBySessionAsync(sessionId);
            
            _logger.LogInformation("Successfully revoked session {SessionId}", sessionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while closing session {SessionId}", sessionId);
            return Result.Failure(ex);
        }
    }

    public async Task<Result> CloseAllSessionsAsync(Guid userId)
    {
        _logger.LogInformation("Attempting to close all active sessions for user {UserId}", userId);

        try
        {
            var updatedRowsCount = await _context.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked)
                .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsRevoked, true));

            if (updatedRowsCount == 0)
            {
                _logger.LogInformation("User {UserId} had no active sessions to close", userId);
                return Result.Success();
            }
            
            await _tokenService.DeactivateAllTokensByUserIdAsync(userId);

            _logger.LogInformation("Successfully revoked all {Count} active sessions for user {UserId}", updatedRowsCount, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while closing all sessions for user {UserId}", userId);
            return Result.Failure(ex);
        }
    }
}