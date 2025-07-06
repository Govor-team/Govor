using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.DTOs;
using Govor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User,Admin")]
public class FriendsController : Controller
{
    private readonly ILogger<FriendsController> _logger;
    private readonly IFriendsService _friendsService;
    private readonly ICurrentUserService _currentUserService;
    
    public FriendsController(ILogger<FriendsController> logger,
        IFriendsService friendsService, 
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _friendsService = friendsService;
        _currentUserService = currentUserService;
    }

    [HttpGet("search")] // api/friends/search?query=
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        try
        {
            var result = await _friendsService.SearchUsersAsync(query, _currentUserService.GetCurrentUserId());
            return Ok(BuildUserDtos(result));
        }
        catch (SearchUsersException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Internal error during user search." });
        }
    }

    [HttpPost("request")]
    public async Task<IActionResult> SendRequest(Guid targetUserId)
    {
        if (targetUserId == Guid.Empty)
            return BadRequest("Target user ID is invalid");

        try
        {
            await _friendsService.SendFriendRequestAsync(_currentUserService.GetCurrentUserId(), targetUserId);
            return Ok(new { message = "Friend request sent successfully." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tried to send request to self");
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (RequestAlreadySentException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Failed to send friend request." });
        }
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetIncomingRequests()
    {
        try
        {
            var result = await _friendsService.GetIncomingRequestsAsync(_currentUserService.GetCurrentUserId());
            return Ok(BuildFriendshipDtos(result));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest("Failed to get friend requests. User data missing.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Internal server error." });
        }
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptFriend([FromQuery] Guid friendshipId)
    {
        if (friendshipId == Guid.Empty)
            return BadRequest("Requester ID is invalid");

        try
        {
            await _friendsService.AcceptFriendRequestAsync(friendshipId, _currentUserService.GetCurrentUserId());
            return Ok(new { message = "Friend request accepted." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Failed to accept friend request." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        try
        {
            var result = await _friendsService.GetFriendsAsync(_currentUserService.GetCurrentUserId());
            return Ok(BuildUserDtos(result));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Ok(Array.Empty<UserDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
    
    private List<UserDto> BuildUserDtos(IEnumerable<User> users) => users.Select(user => new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Description = user.Description,
        WasOnline = user.WasOnline,
        IconId = user.IconId
    }).ToList();

    private List<FriendshipDto> BuildFriendshipDtos(IEnumerable<Friendship> friendships) => friendships.Select(f => new FriendshipDto
    {
        Id = f.Id,
        Status = f.Status,
        AddresseeId = f.AddresseeId,
        RequesterId = f.RequesterId,
        Requester = BuildUserDtos([f.Requester]).First(),
    }).ToList();
}
