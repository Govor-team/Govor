using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.PushTokens;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class PushTokenRepository : IPushTokenRepository
{
    private GovorDbContext _context;

    public PushTokenRepository(GovorDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<string>> GetActiveTokensAsync(Guid userId)
    {
        return await _context.UserPushTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.IsActive)
            .Select(t => t.Token)
            .ToListAsync();
    }

    public async Task<List<string>> GetActiveTokensUsersAsync(IEnumerable<Guid> userIds)
    {
        if (userIds is null || !userIds.Any())
            return [];

        var distinctIds = userIds.Distinct().ToList();

        return await _context.UserPushTokens
            .AsNoTracking()
            .Where(t => distinctIds.Contains(t.UserId) && t.IsActive)
            .Select(t => t.Token)
            .ToListAsync();
    }

    public async Task<string> GetActiveTokenBySessionAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            return "";

        return await _context.UserPushTokens
            .AsNoTracking()
            .Where(t => t.UserSessionId == sessionId && t.IsActive)
            .Select(t => t.Token)
            .FirstOrDefaultAsync() ?? "";
    }

    public async Task AddOrUpdateTokenAsync(Guid userId, Guid? sessionId, string token, string platform, string provider = "FCM")
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (string.IsNullOrWhiteSpace(platform))
            throw new ArgumentException("Platform cannot be empty", nameof(platform));

        var existing = await _context.UserPushTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (existing != null)
        {
            // Updates
            existing.UserId = userId;
            existing.UserSessionId = sessionId;
            existing.Platform = platform;
            existing.Provider = provider;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.LastUsedAt = DateTime.UtcNow;
            existing.Token = token;
            existing.IsActive = true;

            _context.UserPushTokens.Update(existing);
        }
        else
        {
            // Add new 
            var newToken = new UserPushToken
            {
                UserId = userId,
                UserSessionId = sessionId,
                Token = token,
                Platform = platform,
                Provider = provider,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = true,
            };

            _context.UserPushTokens.Add(newToken);
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveTokensAsync(IEnumerable<string> tokens)
    {
        if (tokens is null || !tokens.Any())
            return;

        var tokenList = tokens.Distinct().ToList();

        var toRemove = await _context.UserPushTokens
            .Where(t => tokenList.Contains(t.Token))
            .ToListAsync();

        if (toRemove.Any())
        {
            _context.UserPushTokens.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeactivateTokenBySessionAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            return;

        var tokens = await _context.UserPushTokens
            .Where(t => t.UserSessionId == sessionId && t.IsActive)
            .ToListAsync();

        if (tokens.Any())
        {
            foreach (var token in tokens)
            {
                token.IsActive = false;
                token.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}