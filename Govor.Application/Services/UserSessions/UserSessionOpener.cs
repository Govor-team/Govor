using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.UserSession;
using Govor.Application.Services.Authentication;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Govor.Application.Services.UserSessions;

public class UserSessionOpener : IUserSessionOpener
{
    private readonly IUserSessionsRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserSessionOpener> _logger;
    private readonly JwtRefreshOption _options;

    public UserSessionOpener(IUserSessionsRepository repository, IJwtService jwtService, IOptions<JwtRefreshOption> options, ILogger<UserSessionOpener> logger)
    {
        _jwtService = jwtService;
        _repository = repository;
        _logger = logger;
        _options = options.Value;
    }
    public async Task<string> OpenSessionAsync(User user, string deviceInfo)
    {
        _logger.LogInformation($"Opening session for user {user.Id} on device '{deviceInfo}'");

        var sessions = await _repository.GetByUserIdAsync(user.Id);
        var session = sessions.FirstOrDefault(s => s.DeviceInfo == deviceInfo);

        var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(user);
        var newExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays);

        if (session is not null)
        {
            if (session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
            {
                // Update Session 
                session.RefreshToken = newRefreshToken;
                session.ExpiresAt = newExpiresAt;
                session.CreatedAt = DateTime.UtcNow;
                session.IsRevoked = false;

                await _repository.UpdateAsync(session);
                _logger.LogInformation($"Updated expired/revoked session for user {user.Id} on device '{deviceInfo}'");
            }

            return session.RefreshToken;
        }

        // New Session 
        var newSession = new Core.Models.UserSession
        {
            UserId = user.Id,
            DeviceInfo = deviceInfo,
            RefreshToken = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = newExpiresAt,
            IsRevoked = false
        };

        await _repository.AddAsync(newSession);
        _logger.LogInformation($"Created new session for user {user.Id} on device '{deviceInfo}'");

        return newRefreshToken;
    }
}