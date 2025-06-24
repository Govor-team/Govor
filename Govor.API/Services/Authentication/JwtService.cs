using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Govor.API.Services.Authentication.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Admins;
using Govor.Core.Repositories.Invaites;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Govor.API.Services.Authentication;

public class JwtService : IJwtService
{
    private JwtOption _jwtOption;
    private IInvitesService _invitesService;
    
    public JwtService(IOptions<JwtOption> options, IInvitesService invitesService)
    {
        _jwtOption = options.Value;
        _invitesService = invitesService;
    }
    
    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim("userID", user.Id.ToString()),
            new Claim(ClaimTypes.Role, _invitesService.GetRole(user).Result, ClaimValueTypes.String)
        };
        
        var singing = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.SecretKeу)),
            SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(signingCredentials: singing,
            expires: DateTime.UtcNow.AddHours(_jwtOption.Hours),
            claims: claims);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}