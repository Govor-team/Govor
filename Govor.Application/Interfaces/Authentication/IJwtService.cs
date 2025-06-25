using Govor.Core.Models;

namespace Govor.API.Services.Authentication.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(User user);
}