using Govor.Core.Models;

namespace Govor.API.Services.Authentication.Interfaces;

public interface IJwtService
{
    Task<string> GenerateJwtTokenAsync(User user);
}