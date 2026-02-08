using Govor.Application.Interfaces.Authentication;

namespace Govor.Application.Services.Authentication;

public class JwtTokenHasher : IJwtTokenHasher
{
    public string HashToken(string token)
    {
        return BCrypt.Net.BCrypt.HashPassword(token, workFactor: 13);
    }

    public bool VerifyToken(string token, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(token, storedHash);
    }
}