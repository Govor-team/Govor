namespace Govor.Core.Infrastructure.Extensions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}