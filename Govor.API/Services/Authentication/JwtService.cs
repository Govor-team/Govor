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
    private IInvitesRepository _invitesRepository;
    
    public JwtService(IOptions<JwtOption> options)
    {
        _jwtOption = options.Value;
    }
    
    public string GenerateJwtToken(User user)
    {
        var invite = _invitesRepository.GetByIdAsync(user.InviteId).Result;
        
        var claims = new[]
        {
            new Claim("userID", user.Id.ToString()),
            new Claim(ClaimTypes.Role, invite.IsAdmin ? "Admin" : "User", ClaimValueTypes.String)
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