using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Govor.Application.Authentication.JWT;

public class JwtTokenHasher : IJwtTokenHasher
{
    private readonly byte[] _pepperBytes;

    public JwtTokenHasher(IConfiguration config)
    {
        var pepper = config["EncryptionOption:Secret"]
                     ?? throw new InvalidOperationException("Pepper is missing");

        _pepperBytes = Encoding.UTF8.GetBytes(pepper);
    }

    public string HashToken(string token)
    {
        using var hmac = new HMACSHA256(_pepperBytes);

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hash = hmac.ComputeHash(tokenBytes);

        return Convert.ToBase64String(hash);
    }

    public bool VerifyToken(string token, string storedHash)
    {
        using var hmac = new HMACSHA256(_pepperBytes);

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var computedHash = hmac.ComputeHash(tokenBytes);

        var storedHashBytes = Convert.FromBase64String(storedHash);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
    }
}