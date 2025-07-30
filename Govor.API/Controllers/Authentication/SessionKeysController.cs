using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Authentication;

[RequireHttps]
[ApiController]
[Route("api/session")]
[Authorize(Roles = "Admin, User")]
public class SessionKeysController : Controller
{
    private readonly ILogger<SessionKeysController> _logger;
    private readonly ICurrentUserSessionService _currentSession;
    private readonly ICurrentUserService _currentUser;
    /*
    [HttpPost("keys")]
    public async Task<IActionResult> UploadSessionKeys([FromBody] UploadKeysRequest request)
    {
        var sessionId = _currentSession.GetUserSessionId();

        await _sessionKeyService.AttachKeysAsync(sessionId, request.PublicEncryptionKey, request.PublicSigningKey);

        return Ok();
    }
    
    [Authorize]
    [HttpGet("users/{userId}/keys")]
    public async Task<IActionResult> GetUserPublicKeys(Guid userId)
    {
        var requesterId = _currentUserService.UserId;

        if (!await _friendshipService.AreFriendsAsync(requesterId, userId))
            return Forbid("You can only access keys of your friends");

        var keys = await _sessionKeyService.GetAllActiveKeysAsync(userId);

        return Ok(keys);
    }

    */
}