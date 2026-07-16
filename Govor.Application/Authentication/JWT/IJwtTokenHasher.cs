namespace Govor.Application.Authentication.JWT;

public interface IJwtTokenHasher
{
    string HashToken(string token);
    bool VerifyToken(string token, string storedHash);
}