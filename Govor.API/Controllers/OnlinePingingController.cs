using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Core.Repositories.Users;
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
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest("User can't be found in our database.");
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogError(e, e.Message);
            return Forbid(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, new { error = "Failed to send friend request." });
        }
    }
}