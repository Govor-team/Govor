using Govor.Core.Models.Users;

namespace Govor.Application.Interfaces.Authentication;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(User user);
}