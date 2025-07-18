using System.Security.Claims;
using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces.Authentication;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync(User user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}