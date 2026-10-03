using Govor.Application.Friends;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Users.UserSessions.Crypto;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Govor.Contracts.DTOs;

namespace Govor.API.Controllers.Authentication;

[RequireHttps]
[ApiController]
[Route("api/session")]
[Authorize(Roles = "Admin, User")]
public class SessionKeysController : Controller
{
    private readonly ILogger<SessionKeysController> _logger;
    private readonly IFriendshipService _friendshipService;
    private readonly ICurrentUserSessionService _currentSession;
    private readonly ISessionKeyAttacher _sessionKeyAttacher;
    private readonly ISessionKeysReader _sessionKeysReader;
    private readonly IOneTimePreKeysRotator _oneTimePreKeysRotator;
    private readonly ICurrentUserService _currentUser;

    public SessionKeysController(
        ILogger<SessionKeysController> logger,
        IFriendshipService friendshipService,
        ICurrentUserSessionService currentSession,
        ISessionKeyAttacher sessionKeyAttacher,
        ISessionKeysReader sessionKeysReader,
        ICurrentUserService currentUser,
        IOneTimePreKeysRotator oneTimePreKeysRotator)
    {
        _logger = logger;
        _friendshipService = friendshipService;
        _currentSession = currentSession;
        _sessionKeyAttacher = sessionKeyAttacher;
        _sessionKeysReader = sessionKeysReader;
        _currentUser = currentUser;
        _oneTimePreKeysRotator = oneTimePreKeysRotator;
    }

    [HttpPost("keys")]
    public async Task<IActionResult> UploadSessionKeys([FromBody] UploadKeysRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        if(request.OneTimePreKeys.Count > 100)
            return BadRequest("Too many one time pre keys");
        
        var sessionId = _currentSession.GetUserSessionId();

        await _sessionKeyAttacher.AttachKeysAsync(sessionId,
            request.IdentityKey,
            request.SignedPreKey,
            request.SignedPreKeySignature,
            request.OneTimePreKeys);

        return Ok();
    }
    
    [HttpGet("users/{userId}/keys")]
    public async Task<IActionResult> GetUserPublicKeys(Guid userId)
    {
        var requesterId = _currentUser.GetCurrentUserId();

        if (!(await _friendshipService.GetFriendsAsync(userId)).Select(u => u.Id).Contains(requesterId))
            return Forbid();

        var keys = await _sessionKeysReader.GetAllActiveKeysAsync(userId);

        return Ok(keys.Select(k => new PublicSessionKeysDto
        {
            IdentityKey = Convert.ToBase64String(k.PublicIdentityKey),
            SignedPreKey = k.SignedPreKey is null ? null : new SignedPreKeyDto
            {
                Id = k.SignedPreKey.Id,
                Key = Convert.ToBase64String(k.SignedPreKey.PublicSignedPreKey),
                Signature = Convert.ToBase64String(k.SignedPreKey.SignedPreKeySignature)
            },
            OneTimePreKeys = k.OneTimePreKeys.Where(p => !p.IsUsed).Select(p => new OneTimePreKeyDto
            {
                Id = p.Id, Key = Convert.ToBase64String(p.PublicKey)
            }).ToList()
        }));
    }
    
    [HttpPost("keys/rotate")]
    public async Task<IActionResult> RotateOneTimePreKeys([FromBody] RotateOneTimePreKeysRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.NewOneTimePreKeys.Count > 100)
            return BadRequest("Too many new one time pre keys");
        
        var sessionId = _currentSession.GetUserSessionId();

        await _oneTimePreKeysRotator.RotateOneTimePreKeysAsync(sessionId, request.NewOneTimePreKeys);

        return Ok("One-Time PreKeys rotated successfully.");
    }

    [HttpGet("keys/remaining")]
    public async Task<IActionResult> GetRemainingOneTimePreKeysCount()
    {
        var sessionId = _currentSession.GetUserSessionId();

        var count = await _sessionKeysReader.GetRemainingOneTimePreKeysCountAsync(sessionId);

        return Ok(new { remaining = count });
    }

    [HttpPost("keys/{preKeyId}/used")]
    public async Task<IActionResult> MarkOneTimePreKeyAsUsed([FromRoute] Guid preKeyId)
    {
        var sessionId = _currentSession.GetUserSessionId();

        await _oneTimePreKeysRotator.MarkOneTimePreKeyAsUsedAsync(sessionId, preKeyId);

        return Ok("Marked as used.");
    }

}
