using AutoMapper;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.DTOs;
using Govor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Friends;

[ApiController]
[Authorize]
[Route("api/friends")]
public class FriendsRequestQueryController : Controller
{
    private readonly ILogger<FriendsRequestQueryController> _logger;
    private readonly IFriendRequestQueryService _friendsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    
    public FriendsRequestQueryController(ILogger<FriendsRequestQueryController> logger,
        IFriendRequestQueryService friendsService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
        _friendsService = friendsService;
        _currentUserService = currentUserService;
    }
    
    [HttpGet("requests")] // api/friends/requests
    public async Task<IActionResult> GetIncomingRequests()
    {
        try
        {
            var result = await _friendsService.GetIncomingAsync(_currentUserService.GetCurrentUserId());
            var response = _mapper.Map<List<FriendshipDto>>(result);
            
            return Ok(response);
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

    [HttpGet("responses")]// api/friends/responses 
    public async Task<IActionResult> GetResponses()
    {
        try
        {
            var result = await _friendsService.GetResponsesAsync(_currentUserService.GetCurrentUserId());
            return Ok(BuildFriendshipDtos(result));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Ok(Array.Empty<FriendshipDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
    
    private List<FriendshipDto> BuildFriendshipDtos(IEnumerable<Friendship> friendships) => friendships.Select(f => new FriendshipDto
    {
        Id = f.Id,
        Status = f.Status,
        AddresseeId = f.AddresseeId,
        RequesterId = f.RequesterId,
    }).ToList();
}