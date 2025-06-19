using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Govor.Core.Models;
using Govor.Core.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Govor.API.Services.Authentication;

public class JwtService : IJwtService
{
    private JwtOption _jwtOption;
    
    public JwtService(IOptions<JwtOption> options)
    {
        _jwtOption = options.Value;
    }
    public string GenerateJwtToken(User user)
    {
        Claim[] claims = [new("userID", user.Id.ToString())];
        
        var singing = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.SecretKeу)),
            SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(signingCredentials: singing,
            expires: DateTime.UtcNow.AddHours(_jwtOption.Hours),
            claims: claims);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}