using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.UserSession;
using Govor.Application.Services.Authentication;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Govor.Application.Services.UserSessions;

public class UserSessionOpener : IUserSessionOpener
{
    private readonly IUserSessionsRepository _repository;
    private readonly ILogger<UserSessionOpener> _logger;
    private readonly IJwtTokenHasher _jwtTokenHasher;
    private readonly JwtRefreshOption _options;
    private readonly IJwtService _jwtService;
    
    public UserSessionOpener(
        IUserSessionsRepository repository,
        IJwtService jwtService,
        IJwtTokenHasher jwtTokenHasher,
        IOptions<JwtRefreshOption> options,
        ILogger<UserSessionOpener> logger)
    {
        _jwtService = jwtService;
        _repository = repository;
        _jwtTokenHasher = jwtTokenHasher;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<RefreshResult> OpenSessionAsync(User user, string deviceInfo)
    {
        _logger.LogInformation($"Opening session for user {user.Id} on device '{deviceInfo}'");

        try
        {
            var sessions = await _repository.GetByUserIdAsync(user.Id);
            var existingSession = sessions.FirstOrDefault(s => s.DeviceInfo == deviceInfo);

            if (existingSession is not null)
                return await UpdateExistingSessionAsync(user, deviceInfo, existingSession);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
           
        }
        
        return await CreateNewSessionAsync(user, deviceInfo);
    }
    
    private async Task<RefreshResult> UpdateExistingSessionAsync(User user, string deviceInfo, UserSession session)
    {
        var (accessToken, refreshToken) = await GenerateTokensAsync(user, session.Id);

        var newTokenHash = _jwtTokenHasher.HashToken(refreshToken);
        var newExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays);

        session.RefreshTokenHash = newTokenHash;
        session.ExpiresAt = newExpiresAt;
        session.CreatedAt = DateTime.UtcNow;
        session.IsRevoked = false;

        await _repository.UpdateAsync(session);
        _logger.LogInformation($"Updated session for user {user.Id} on device '{deviceInfo}'");
        
        return new RefreshResult(refreshToken, accessToken);
    }

    private async Task<RefreshResult> CreateNewSessionAsync(User user, string deviceInfo)
    {
        var sessionId = Guid.NewGuid();
        var (accessToken, refreshToken) = await GenerateTokensAsync(user, sessionId);

        var refreshTokenHash = _jwtTokenHasher.HashToken(refreshToken);

        var newSession = new UserSession
        {
            Id = sessionId,
            UserId = user.Id,
            DeviceInfo = deviceInfo,
            RefreshTokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
            IsRevoked = false
        };

        await _repository.AddAsync(newSession);

        _logger.LogInformation($"Created new session {sessionId} for user {user.Id} on device '{deviceInfo}'");

        return new RefreshResult(refreshToken, accessToken);
    }
    
    private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user, Guid sessionId)
    {
        var accessToken = await _jwtService.GenerateAccessTokenAsync(user, sessionId);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user);
        return (accessToken, refreshToken);
    }
}