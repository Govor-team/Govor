using System.Security.Claims;
using Govor.Domain.Models.Users;

namespace Govor.Application.Authentication.JWT;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(User user, Guid sessionId);
    Task<string> GenerateRefreshTokenAsync(User user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}