using AutoMapper;
using Govor.Application.Friends;
using Govor.Application.Infrastructure.Extensions;
using Govor.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Friends;

[Route("api/friends")]
public class FriendshipController : Controller
{
    private readonly ILogger<FriendshipController> _logger;
    private readonly IFriendshipService _friendsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    
    public FriendshipController(
        ILogger<FriendshipController> logger,
        IFriendshipService friendsService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
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
            
            var response = _mapper.Map<List<UserDto>>(result);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
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
            
            var response = _mapper.Map<List<UserDto>>(result);
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Ok(Array.Empty<UserDto>());
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
}