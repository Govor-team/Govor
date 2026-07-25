using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Users.UserSessions;

public class UserSessionReader : IUserSessionReader
{
    private readonly GovorDbContext _context;
    private readonly ILogger<UserSessionReader> _logger;

    public UserSessionReader(GovorDbContext context, ILogger<UserSessionReader> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<List<UserSession>, Error>> GetAllSessionsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<List<UserSession>>(Error.Conflict("UserSession.InvalidUserId",
                "Provided User ID cannot be empty.")
            );

        }

        _logger.LogInformation("Getting all active sessions for user {UserId}", userId);
            
        try
        {
            var sessions = await _context.UserSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId && !s.IsRevoked)
                .ToListAsync();
            
            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user sessions for user {UserId}", userId);
            return Result.Failure<List<UserSession>>(ex);
        }
    }
}