using Govor.Application.Authentication.JWT;
using Govor.Domain;
using Govor.Domain.Common; // Путь к вашему Result и Error
using Govor.Domain.Models.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Govor.Application.Users.UserSessions;

public class UserSessionOpener : IUserSessionOpener
{
    private readonly ILogger<UserSessionOpener> _logger;
    private readonly IUserSessionReader _userSessionReader;
    private readonly GovorDbContext _context; 
    private readonly IJwtTokenHasher _jwtTokenHasher;
    private readonly JwtRefreshOption _options;
    private readonly IJwtService _jwtService;
    
    public UserSessionOpener(
        ILogger<UserSessionOpener> logger,
        IUserSessionReader userSessionReader,
        GovorDbContext context,
        IJwtTokenHasher jwtTokenHasher,
        IOptions<JwtRefreshOption> options,
        IJwtService jwtService)
    {
        _logger = logger;
        _userSessionReader = userSessionReader;
        _context = context;
        _jwtTokenHasher = jwtTokenHasher;
        _options = options.Value;
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshResult>> OpenSessionAsync(User user, string deviceInfo)
    {
        _logger.LogInformation("Opening session for user {UserId} on device '{DeviceInfo}'", user.Id, deviceInfo);
        
        var result = await _userSessionReader.GetAllSessionsAsync(user.Id);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to fetch sessions for user {UserId}: {Error}", user.Id, result.Error.Message);
            return Result<RefreshResult>.Failure(result.Error);
        }
        
        var sessions = result.Value;
        var existingSession = sessions.FirstOrDefault(s => s.DeviceInfo == deviceInfo);

        try
        {
            RefreshResult refreshResult;

            if (existingSession is not null)
            {
                refreshResult = await UpdateExistingSessionAsync(user, deviceInfo, existingSession);
            }
            else
            {
                refreshResult = await CreateNewSessionAsync(user, deviceInfo);
            }
            
            await _context.SaveChangesAsync();
            
            return refreshResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error while opening session for user {UserId}", user.Id);
            return Result<RefreshResult>.Failure(ex);
        }
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
        
        _logger.LogInformation("Updated session for user {UserId} on device '{DeviceInfo}'", user.Id, deviceInfo);
        
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
        
        await _context.UserSessions.AddAsync(newSession);

        _logger.LogInformation("Created new session {SessionId} for user {UserId} on device '{DeviceInfo}'", sessionId, user.Id, deviceInfo);

        return new RefreshResult(refreshToken, accessToken);
    }
    
    private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user, Guid sessionId)
    {
        var accessToken = await _jwtService.GenerateAccessTokenAsync(user, sessionId);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user);
        return (accessToken, refreshToken);
    }
}