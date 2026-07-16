using Govor.Application.Infrastructure.Extensions;
using Govor.Application.PushNotifications;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin, User")]
[Route("api")]
public class PushTokensController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentUserSessionService _currentSession;
    private readonly IPushTokenService _pushTokenService;

    public PushTokensController(
        IPushTokenService  pushTokenService,
        ICurrentUserService currentUser,
        ICurrentUserSessionService currentSession)
    {
        _pushTokenService = pushTokenService;
        _currentUser = currentUser;
        _currentSession = currentSession;
    }

    [HttpPost("pushes/token/register")]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterPushTokenRequest req)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(req.Token) || string.IsNullOrWhiteSpace(req.Platform))
                return BadRequest(ModelState);

            var currentId = _currentUser.GetCurrentUserId();
            var currentSessionId = _currentSession.GetUserSessionId();

            await _pushTokenService.AddOrUpdateTokenAsync(
                userId: currentId,
                sessionId: currentSessionId,
                token: req.Token,
                platform: req.Platform);

            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
}