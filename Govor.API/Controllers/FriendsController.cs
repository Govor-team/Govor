using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User,Admin")]
public class FriendsController : Controller
{
    private readonly ILogger<FriendsController> _logger;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromBody] string query)
    {
        return BadRequest("Not a valid request");
    }

    [HttpPost("request")]
    public async Task<IActionResult> SendRequest([FromBody] Guid targetUserId)
    {
        return BadRequest("Not a valid request");
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetIncomingRequests()
    {
        return BadRequest("Not a valid request");
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptFriend([FromBody] Guid requesterId)
    {
        return BadRequest("Not a valid request");
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        return BadRequest("Not a valid request");
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User?.FindFirst("userID")?.Value;
        _logger.LogInformation("Claims: {Claims}", string.Join(", ", HttpContext.User?.Claims.Select(c => $"{c.Type}: {c.Value}") ?? new string[0]));
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogError("No userID claim found");
            return Guid.Empty;
        }
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}