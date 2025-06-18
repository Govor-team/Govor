using Govor.Core.Infrastructure.Extensions;

namespace Govor.API.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(hashedPassword, providedPassword);
    }
}