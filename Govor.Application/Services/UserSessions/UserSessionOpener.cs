using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.UserSession;
using Govor.Application.Services.Authentication;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Govor.Application.Services.UserSessions
{
    public class UserSessionOpener : IUserSessionOpener
    {
        private readonly IUserSessionsRepository _repository;
        private readonly ILogger<UserSessionOpener> _logger;
        private readonly JwtRefreshOption _options;
        private readonly IJwtService _jwtService;

        public UserSessionOpener(
            IUserSessionsRepository repository,
            IJwtService jwtService,
            IOptions<JwtRefreshOption> options,
            ILogger<UserSessionOpener> logger)
        {
            _jwtService = jwtService;
            _repository = repository;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<RefreshResult> OpenSessionAsync(User user, string deviceInfo)
        {
            _logger.LogInformation($"Opening session for user {user.Id} on device '{deviceInfo}'");

            try
            {
                var sessions = await _repository.GetByUserIdAsync(user.Id);
                var session = sessions.FirstOrDefault(s => s.DeviceInfo == deviceInfo);
                
                if (session is not null)
                {
                    var newExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays);
                    var accessToken = await _jwtService.GenerateAccessTokenAsync(user, session.Id);
                    var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync(user);
                    
                    session.RefreshToken = newRefreshToken;
                    session.ExpiresAt = newExpiresAt;
                    session.CreatedAt = DateTime.UtcNow;
                    session.IsRevoked = false;

                    await _repository.UpdateAsync(session);
                    _logger.LogInformation($"Updated session for user {user.Id} on device '{deviceInfo}'");
                    
                    return new RefreshResult(session.RefreshToken, accessToken);
                }
            
                return await OpenNewSession();
            }
            catch (NotFoundByKeyException<Guid> ex)
            {
                return await OpenNewSession();
            }
        
            async Task<RefreshResult> OpenNewSession()
            {
                var sessionId = Guid.NewGuid(); 

                var accessToken = await _jwtService.GenerateAccessTokenAsync(user, sessionId);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user);

                var newSession = new UserSession
                {
                    Id = sessionId,
                    UserId = user.Id,
                    DeviceInfo = deviceInfo,
                    RefreshToken = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays),
                    IsRevoked = false
                };

                await _repository.AddAsync(newSession);

                _logger.LogInformation($"Created new session {sessionId} for user {user.Id} on device '{deviceInfo}'");

                return new RefreshResult(refreshToken, accessToken);
            }

        }
    }
}

