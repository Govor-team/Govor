using Govor.Application.Interfaces.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

namespace Govor.Application.Infrastructure.Extensions;

public class CurrentUserSessionService : ICurrentUserSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserSessionId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst("sid")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Session id (sid) claim is missing or invalid");
        }

        return userId;
    }
}