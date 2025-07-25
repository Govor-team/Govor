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
    private readonly IJwtService _jwtService;

    public UserSessionRefresher(
        IUserSessionsRepository sessionsRepository,
        ILogger<UserSessionRefresher> logger,
        IUsersRepository usersRepository,
        IOptions<JwtRefreshOption> options,
        IJwtService jwtService)
    {
        _sessionsRepository = sessionsRepository;
        _logger = logger;
        _usersRepository = usersRepository;
        _options = options.Value;
        _jwtService = jwtService;
    }

    public async Task<RefreshResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var session = await _sessionsRepository.GetByRefreshTokenAsync(refreshToken);

            if (session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token is invalid or expired");
            
            session.IsRevoked = true;
            await _sessionsRepository.UpdateAsync(session);

            // Find user 
            var user = await _usersRepository.FindByIdAsync(session.UserId);

            // New tokens 
            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user, session.Id);
            var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(user);

            // Opening new session 
            var newSession = new UserSession
            {
                UserId = user.Id,
                RefreshToken = newRefreshToken,
                DeviceInfo = session.DeviceInfo,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays)
            };

            await _sessionsRepository.AddAsync(newSession);

            return new RefreshResult(newRefreshToken, newAccessToken);
        }
        catch (NotFoundByKeyException<string> ex)
        {
            _logger.LogWarning(ex, ex.Message);
            throw new UnauthorizedAccessException("Invalid refresh token", ex);
        }
    }
}