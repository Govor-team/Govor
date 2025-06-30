using System.Security.Claims;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

namespace Govor.Application.Infrastructure.Extensions;

public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal _user;
    
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext.User;
    }
    
    public Guid GetCurrentUserId()
    {
        var userIdClaim = _user.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("userID claim is missing or invalid");
        }
        return userId;
    }
}