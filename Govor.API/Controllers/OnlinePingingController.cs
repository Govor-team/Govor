using Govor.Application.Infrastructure.Extensions;
using Govor.Application.PingHandler;
using Govor.Application.Users.UserOnlineStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/online")]
[Authorize(Roles = "User,Admin")]
public class OnlinePingingController : Controller
{
    private readonly ILogger<OnlinePingingController> _logger;
    private readonly IPingHandlerService _ping;
    private readonly IUserPresenceReader _presenceReader;
    private readonly IOnlineUserStore _userOnlineStore;
    private readonly ICurrentUserService _currentUserService;

    public OnlinePingingController(ILogger<OnlinePingingController> logger, 
        IPingHandlerService ping,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _ping = ping;
        _currentUserService = currentUserService;
    }
    
    
    [HttpPatch("ping")]// api/online/ping 
    public async Task<IActionResult> Ping()
    {
        try
        {
            _logger.LogInformation("Ping...");
            
            var id = _currentUserService.GetCurrentUserId();
            await _ping.Ping(id);
            
            _logger.LogInformation($"Ping from user {id} processed successfully");
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, "Failed to ping.");
        }
    }

    [HttpGet("status/{userId}")]
    public async Task<IActionResult> GetStatus(Guid userId)
    {
        try
        {
            var isOnline =  _userOnlineStore.IsOnline(userId);
            var lastSeen = await _presenceReader.GetLastSeenAsync(userId);

            return Ok(new {
                isOnline,
                lastSeen
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, "Internal server error.");
        }
    }
}