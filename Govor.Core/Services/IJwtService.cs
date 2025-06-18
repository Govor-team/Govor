using Govor.Core.Models;

namespace Govor.Core.Services;

public interface IJwtService
{
    string GenerateJwtToken(User user);
}