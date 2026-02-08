using Govor.Application.Interfaces.UserSession;
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
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (string.IsNullOrEmpty(refreshRequest.RefreshToken))
                throw new InvalidOperationException("Refresh token cant be empty.");
            
            var result = await _userSession.RefreshTokenAsync(refreshRequest.RefreshToken);

            return Ok(new RefreshTokenResponse()
                {
                    AccessToken = result.accessToken,
                    RefreshToken = result.refreshToken
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid refresh token.");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Refresh token failed.");
            return Unauthorized("Invalid refresh token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}