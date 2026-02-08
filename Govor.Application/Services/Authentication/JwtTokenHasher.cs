using System.Security.Cryptography;
using System.Text;
using Govor.Application.Interfaces.Authentication;
using Microsoft.Extensions.Configuration;

namespace Govor.Application.Services.Authentication;

public class JwtTokenHasher : IJwtTokenHasher
{
    private readonly string _pepper;

    public JwtTokenHasher(IConfiguration config)
    {
        _pepper = config["EncryptionOption:Secret"] ?? "D1fault%Lxng%Randxm^Secret^Key(123!";
    }

    public string HashToken(string token)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_pepper));
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyToken(string token, string storedHash)
    {
        var currentHashBytes = Encoding.UTF8.GetBytes(HashToken(token));
        var storedHashBytes = Encoding.UTF8.GetBytes(storedHash);
        
        return CryptographicOperations.FixedTimeEquals(currentHashBytes, storedHashBytes);
    }
}