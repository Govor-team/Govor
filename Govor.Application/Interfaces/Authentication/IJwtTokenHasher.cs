namespace Govor.Application.Interfaces.Authentication;

public interface IJwtTokenHasher
{
    string HashToken(string token);
    bool VerifyToken(string token, string storedHash);
}