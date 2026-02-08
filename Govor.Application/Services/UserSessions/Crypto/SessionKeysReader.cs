using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.UserSession.Crypto;
using Govor.Core.Models.Users.Crypto;
using Govor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.UserSessions.Crypto;

public class SessionKeysReader : ISessionKeysReader
{
    private readonly ILogger<SessionKeyAttacher> _logger;
    private readonly GovorDbContext _context;

    public SessionKeysReader(ILogger<SessionKeyAttacher> logger, GovorDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task<bool> HasKeysAttachedAsync(Guid sessionId)
    {
        return await _context.UserCryptoSessions.AnyAsync(c => c.UserSessionId == sessionId);
    }

    public async Task<IReadOnlyList<UserCryptoSession>> GetAllActiveKeysAsync(Guid userId)
    {
        _logger.LogInformation("Getting all active keys for user {UserId}.", userId);
        
        var now = DateTime.UtcNow;

        return await _context.UserCryptoSessions
            .AsNoTracking()
            .Include(c => c.OneTimePreKeys)
            .Include(c => c.UserSession)
            .Where(c => c.UserSession.UserId == userId
                        && !c.UserSession.IsRevoked
                        && c.UserSession.ExpiresAt > now)
            .ToListAsync();
    }

    public async Task<int> GetRemainingOneTimePreKeysCountAsync(Guid sessionId)
    {
        _logger.LogInformation("Getting count of one time pre keys for session {UserId}.", sessionId);

        return await _context.OneTimePreKeys
            .AsNoTracking()
            .Include(f => f.UserCryptoSession)
            .CountAsync(f => f.UserCryptoSession.UserSessionId == sessionId);
    }

    public async Task<UserCryptoSession?> GetKeysBySessionIdAsync(Guid sessionId)
    {
        _logger.LogInformation("Getting keys for session {session}.", sessionId);
        
        return await _context.UserCryptoSessions
            .AsNoTracking()
            .Include(c => c.OneTimePreKeys)
            .Include(c => c.UserSession)
            .FirstOrDefaultAsync(c => c.UserSessionId == sessionId);
    }
}