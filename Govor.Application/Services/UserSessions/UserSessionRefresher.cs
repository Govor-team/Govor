using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.UserSession;
using Govor.Application.Services.Authentication;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Govor.Application.Services.UserSessions;

public class UserSessionRefresher : IUserSessionRefresher
{
    private readonly IUserSessionsRepository _sessionsRepository;
    private readonly ILogger<UserSessionRefresher> _logger;
    private readonly IUsersRepository _usersRepository;
    private readonly JwtRefreshOption _options;
    private readonly IJwtTokenHasher _jwtTokenHasher;
    private readonly IJwtService _jwtService;

    public UserSessionRefresher(
        IUserSessionsRepository sessionsRepository,
        ILogger<UserSessionRefresher> logger,
        IUsersRepository usersRepository,
        IOptions<JwtRefreshOption> options,
        IJwtTokenHasher jwtTokenHasher,
        IJwtService jwtService)
    {
        _sessionsRepository = sessionsRepository;
        _logger = logger;
        _usersRepository = usersRepository;
        _options = options.Value;
        _jwtTokenHasher = jwtTokenHasher;
        _jwtService = jwtService;
    }

    public async Task<RefreshResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var session = await _sessionsRepository.GetByHashedRefreshTokenAsync(_jwtTokenHasher.HashToken(refreshToken));

            if (session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token is invalid or expired");

            // Find user 
            var user = await _usersRepository.FindByIdAsync(session.UserId);

            // New tokens 
            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user, session.Id);
            var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(user);
            
            var newRefreshTokenHash = _jwtTokenHasher.HashToken(newRefreshToken);
            
            // Opening new session 
            var newSession = new UserSession
            {
                Id = session.Id,
                UserId = user.Id,
                RefreshTokenHash = newRefreshTokenHash,
                DeviceInfo = session.DeviceInfo,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays)
            };

            await _sessionsRepository.UpdateAsync(newSession);

            return new RefreshResult(newRefreshToken, newAccessToken);
        }
        catch (NotFoundByKeyException<string> ex)
        {
            _logger.LogWarning(ex, ex.Message);
            throw new UnauthorizedAccessException("Invalid refresh token", ex);
        }
    }
}