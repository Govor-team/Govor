using Govor.Contracts.DTOs;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class FriendshipsController : Controller
{
    private readonly ILogger<FriendshipsController> _logger;
    private readonly IFriendshipsRepository _friendshipsRepository;

    public FriendshipsController(ILogger<FriendshipsController> logger, IFriendshipsRepository friendshipsRepository)
    {
        _logger = logger;
        _friendshipsRepository = friendshipsRepository;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            _logger.LogInformation("Get all friendships by administrator");
            var result = await _friendshipsRepository.GetAllAsync();
            return Ok(BuildFriendshipDtos(result));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, new { error = "Internal server error." });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        if(userId == Guid.Empty)
            return BadRequest("User id can't be empty.");

        try
        {
            _logger.LogInformation($"Get user's {userId} all friendships by administrator");
            var result = await _friendshipsRepository.FindByUserIdAsync(userId);
            return Ok(BuildFriendshipDtos(result));
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> RemoveFriendship(Guid Id)
    {
        if(Id == Guid.Empty)
            return BadRequest("FS id can't be empty.");
        try
        {
            var result = await _friendshipsRepository.GetByIdAsync(Id);
            await _friendshipsRepository.RemoveAsync(result);
            return Ok();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(ex.Message);
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
    }).ToList();
}