using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces;
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

    public FriendsController(ILogger<FriendsController> logger, IFriendsService friendsService)
    {
        _logger = logger;
        _friendsService = friendsService;
    }
    
    [HttpGet("search")] //api/friends/search
    public async Task<IActionResult> Search(string query)
    {
        try
        {
           var result = await _friendsService.SearchUsersAsync(query, GetCurrentUserId());
           
           return Ok(BuildUserDtos(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest("Something happened during the search... try again");
        }
    }

    [HttpPost("request")] //api/friends/request
    public async Task<IActionResult> SendRequest(Guid targetUserId)
    {
        try
        {
           await _friendsService.SendFriendRequestAsync(targetUserId, GetCurrentUserId());
           return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("The user tried to send a friend request to themselves");
            return BadRequest("You can't send a friend request to themselves");
        }
        catch (RequestAlreadySentException ex)
        {
            _logger.LogWarning("The user tried to send a friend request but the request is already sent");
            return BadRequest("You already sent a friend request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest("Something happened during the request... try again");
        }
    }

    [HttpGet("requests")] //api/friends/requests
    public async Task<IActionResult> GetIncomingRequests()
    {
        try
        {
            _logger.LogInformation($"Getting incoming requests for user {GetCurrentUserId()}...");
            var result = await _friendsService.GetIncomingRequestsAsync(GetCurrentUserId());
            return Ok(BuildFriendshipDtos(result));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest("You can't get real friend requests because we can't find your user data! Try again");
        }
    }

    [HttpPost("accept")] //api/friends/accept
    public async Task<IActionResult> AcceptFriend(Guid requesterId)
    {
        try
        {
            await _friendsService.AcceptFriendRequestAsync(requesterId, GetCurrentUserId());
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("The user tried to accept a friend request but the request not exists");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("User tried to accept a friend request but addresseeId not equal userId");
            return BadRequest("You can't accept a friend request");
        }
    }

    [HttpGet] //api/friends/
    public async Task<IActionResult> GetFriends()
    {
        try
        {
            _logger.LogInformation($"Getting friends by user {GetCurrentUserId()}...");
            var result = await _friendsService.GetFriendsAsync(GetCurrentUserId());
            
            return Ok(BuildUserDtos(result));
        }
        catch(UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest("Something happened during the request... try again");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User?.FindFirst("userID")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("userID claim is missing or invalid");
        }
        return userId;
    }
    
    private List<UserDto> BuildUserDtos(IEnumerable<User> users)
    {
        List<UserDto> userDtos = new List<UserDto>();
           
        foreach (var user in users)
        {
            userDtos.Add(new UserDto()
            {
                Id = user.Id,
                Description = user.Description,
                Username = user.Username,
                WasOnline = user.WasOnline,
                IconId = user.IconId,
            });
        }
        return userDtos;
    }
    
    private List<FriendshipDto> BuildFriendshipDtos(List<Friendship> result)
    {
        List<FriendshipDto> dtos = new List<FriendshipDto>();
        
        foreach (var friendship in result)
        {
            dtos.Add(new FriendshipDto()
            {
                Id = friendship.Id,
                Status = friendship.Status,
                AddresseeId =  friendship.AddresseeId,
                RequesterId = friendship.RequesterId,
            });    
        }
        
        return dtos;
    }
}