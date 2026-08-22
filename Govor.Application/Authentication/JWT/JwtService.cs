using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Govor.Domain.Models.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Govor.Application.Authentication.JWT;

public class JwtService : IJwtService
{
    private JwtAccessOption _jwtAccessOption;
    private JwtRefreshOption _refreshOptions;
    private IInvitesService _invitesService;
    
    public JwtService(IOptions<JwtAccessOption> options, IOptions<JwtRefreshOption> refreshOptions, IInvitesService invitesService)
    {
        _refreshOptions = refreshOptions.Value;
        _jwtAccessOption = options.Value;
        _invitesService = invitesService;
    }
    
    public async Task<string> GenerateAccessTokenAsync(User user, Guid sessionId)
    {
        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("sid", sessionId.ToString()),
            new Claim(ClaimTypes.Role, await _invitesService.GetRoleNameAsync(user))
            //new Claim(ClaimTypes.Role, await _invitesService.GetRoleNameAsync(user), ClaimValueTypes.String)
        };
        
        var singing = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAccessOption.SecretKey)),
            SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(_jwtAccessOption.Minutes),
            signingCredentials: singing,
            claims: claims);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAccessOption.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("tokenType", "refresh")
        };

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddDays(_refreshOptions.RefreshTokenLifetimeDays),
            signingCredentials: creds,
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAccessOption.SecretKey)),
            ValidateLifetime = false // << important 
        };
        
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

}