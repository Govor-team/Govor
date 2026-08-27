using Govor.Application.Infrastructure.Common;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.PushNotifications;

public class PushTokenService : IPushTokenService
{
    private readonly GovorDbContext _context;
    private readonly ILogger<PushTokenService> _logger;
    private readonly INowDateTimeProvider _nowDateTimeProvider;

    public PushTokenService(GovorDbContext context,
        INowDateTimeProvider nowDateTimeProvider,
        ILogger<PushTokenService> logger)
    {
        _nowDateTimeProvider = nowDateTimeProvider;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Unit, Error>> DeactivateTokenBySessionAsync(Guid sessionId)
    {
        try
        {
            await _context.UserPushTokens
                .Where(t => t.UserSessionId == sessionId && t.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate push token for session {SessionId}", sessionId);
            return Result.Failure(ex);
        }
    }

    public async Task<Result<Unit, Error>> DeactivateAllTokensByUserIdAsync(Guid userId)
    {
        try
        {
            await _context.UserPushTokens
                .Where(t => t.UserId == userId && t.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate all push tokens for user {UserId}", userId);
            return Result.Failure(ex);
        }
    }

    public async Task<Result<List<string>, Error>> GetStringsActiveTokensAsync(Guid userId)
    {
        try
        {
            var tokens = await _context.UserPushTokens
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.IsActive)
                .Select(t => t.Token)
                .ToListAsync();

            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch active push tokens for user {UserId}", userId);
            return Result.Failure<List<string>>(ex);
        }
    }

    public async Task<Result<List<string>, Error>> GetUsersStringsActiveTokensAsync(IEnumerable<Guid> userIds)
    {
        try
        {
            var tokens = await _context.UserPushTokens
                .AsNoTracking()
                .Where(t => userIds.Contains(t.UserId) && t.IsActive)
                .Select(t => t.Token)
                .ToListAsync();

            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch active push tokens for bulk users");
            return Result.Failure<List<string>>(ex);
        }
    }

    public async Task<Result<string?, Error>> GetActiveTokenBySessionAsync(Guid sessionId)
    {
        try
        {
            var token = await _context.UserPushTokens
                .AsNoTracking()
                .Where(t => t.UserSessionId == sessionId && t.IsActive)
                .Select(t => t.Token)
                .FirstOrDefaultAsync();

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch active push token for session {SessionId}", sessionId);
            return Result.Failure<string?>(ex);
        }
    }

    public async Task<Result<Unit, Error>> RemoveTokensAsync(IEnumerable<string> tokens)
    {
        if (tokens is null || !tokens.Any()) 
            return Result.Success();

        try
        {
            await _context.UserPushTokens
                .Where(t => tokens.Contains(t.Token))
                .ExecuteDeleteAsync();
            
            await _context.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk remove invalid push tokens");
            return Result.Failure(ex);
        }
    }

    public async Task<Result<Unit, Error>> AddOrUpdateTokenAsync(Guid userId, Guid sessionId, string token, string platform)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result<Unit, Error>.Failure(Error.Failure("PushToken.Empty", "Push token cannot be empty."));
        }
        
        var existingToken = await _context.UserPushTokens
            .FirstOrDefaultAsync(t => t.Platform == platform && t.UserId == userId && t.UserSessionId == sessionId);

        if (existingToken is null)
        {
            var newToken = new UserPushToken 
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserSessionId = sessionId,
                Token = token,
                Platform = platform,
                CreatedAt = _nowDateTimeProvider.Now
            };

            await _context.UserPushTokens.AddAsync(newToken);
        }
        else
        {
            existingToken.UserId = userId;
            existingToken.UserSessionId = sessionId;
            existingToken.Platform = platform;
            existingToken.Token = token;
            existingToken.UpdatedAt = _nowDateTimeProvider.Now;
        }
        
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
