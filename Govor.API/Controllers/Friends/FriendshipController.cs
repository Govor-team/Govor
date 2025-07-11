using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.DTOs;
using Govor.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Friends;

[Route("api/friends")]
public class FriendshipController : Controller
{
    private readonly ILogger<FriendshipController> _logger;
    private readonly IFriendshipService _friendsService;
    //private readonly IUserDtoBuilder _builder;
    private readonly ICurrentUserService _currentUserService;

    public FriendshipController(ILogger<FriendshipController> logger,
        IFriendshipService friendsService,
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
    
    [HttpGet] // api/friends 
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

}