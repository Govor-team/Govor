#nullable enable
using System.Security.Claims;
using Govor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Common.Extensions;

public static class SessionAuthorization
{
    public static Task<bool> HasActiveSessionAsync(this GovorDbContext context, ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true || user.FindFirst("tokenType")?.Value == "refresh" ||
            !Guid.TryParse(user.FindFirst("userId")?.Value, out var userId) ||
            !Guid.TryParse(user.FindFirst("sid")?.Value, out var sessionId))
            return Task.FromResult(false);

        var now = DateTime.UtcNow;
        return context.UserSessions.AsNoTracking().AnyAsync(s => s.Id == sessionId &&
            s.UserId == userId && !s.IsRevoked && s.ExpiresAt > now);
    }
}
