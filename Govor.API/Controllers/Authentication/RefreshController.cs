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
        if (string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
        {
            _logger.LogWarning("Refresh request failed: token is empty.");
            return BadRequest("Refresh token can't be empty.");
        }
        
        var result = await _userSession.RefreshTokenAsync(refreshRequest.RefreshToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Refresh token failed. Error Code: {Code}", result.Error.Code);
            
            return result.Error.Code switch
            {
                "Auth.EmptyToken" => BadRequest(result.Error.Message),
                "Auth.InvalidToken" => Unauthorized(result.Error.Message),
                _ => BadRequest($"Refresh failed: {result.Error.Message}")
            };
        }

        return Ok(new RefreshTokenResponse
        {
            AccessToken = result.Value.accessToken,
            RefreshToken = result.Value.refreshToken
        });
    }
}