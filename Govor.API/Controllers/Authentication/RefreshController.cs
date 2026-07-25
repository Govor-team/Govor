using Govor.API.Common.Extensions;
using Govor.Application.Users.UserSessions;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Authentication;

[ApiController]
[AllowAnonymous]
[Route("api/auth/token")]
public class RefreshController : Controller
{
    private readonly ILogger<RefreshController> _logger;
    private readonly IUserSessionRefresher _userSession;

    public RefreshController(
        ILogger<RefreshController> logger,
        IUserSessionRefresher userSession)
    {
        _logger = logger;
        _userSession = userSession;
    }

    //[RequireHttps] 
    [HttpPost("refresh")] // api/auth/token/refresh
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshRequest)
    {
        var result = await _userSession.RefreshTokenAsync(refreshRequest.RefreshToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Refresh token failed. Error Code: {Code}", result.Error.Code);
        }

        return result.ToActionResult();
    }
}