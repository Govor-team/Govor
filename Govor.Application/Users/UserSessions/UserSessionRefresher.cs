using Govor.Application.Authentication.JWT;
using Govor.Domain;
using Govor.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartRes;

namespace Govor.Application.Users.UserSessions;

public class UserSessionRefresher : IUserSessionRefresher
{
    private readonly ILogger<UserSessionRefresher> _logger;
    private readonly JwtRefreshOption _options;
    private readonly IJwtTokenHasher _jwtTokenHasher;
    private readonly IJwtService _jwtService;
    private readonly GovorDbContext _context;

    public UserSessionRefresher(
        ILogger<UserSessionRefresher> logger,
        IOptions<JwtRefreshOption> options,
        IJwtTokenHasher jwtTokenHasher,
        IJwtService jwtService,
        GovorDbContext context)
    {
        _logger = logger;
        _options = options.Value;
        _jwtTokenHasher = jwtTokenHasher;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<Result<RefreshResult, Error>> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<RefreshResult>(Error.Failure("Auth.EmptyToken", "Refresh token can't be empty."));
        }
        
        var hashedToken = _jwtTokenHasher.HashToken(refreshToken);

        try
        {
            var session = await _context.UserSessions
                .Include(userSession => userSession.User)
                .FirstOrDefaultAsync(s => s.RefreshTokenHash == hashedToken);
            
            if (session is null)
            {
                _logger.LogWarning("Refresh token session not found for hashed token");
                return Result.Failure<RefreshResult>(Error.Failure("Auth.InvalidToken", "Invalid refresh token."));
            }
            
            if (session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Attempted to refresh an expired or revoked session: {SessionId}", session.Id);
                return Result.Failure<RefreshResult>(Error.Failure("Auth.InvalidToken", "Refresh token is invalid or expired."));
            }

            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(session.User, session.Id);
            var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(session.User);
            var newRefreshTokenHash = _jwtTokenHasher.HashToken(newRefreshToken);
            
            session.RefreshTokenHash = newRefreshTokenHash;
            session.CreatedAt = DateTime.UtcNow;
            session.ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays);
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully refreshed session {SessionId} for user {UserId}", session.Id, session.UserId);

            return new RefreshResult(newRefreshToken, newAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error occurred during token refresh execution");
            return Result.Failure<RefreshResult>(ex);
        }
    }
}
