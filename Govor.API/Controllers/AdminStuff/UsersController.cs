using Govor.Application.Interfaces;
using Govor.Contracts.Responses.Admins;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;


[ApiController] 
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUsersAdministration _users;

    
    public UsersController(ILogger<UsersController> logger,
        IUsersAdministration users,
        IInvitationGenerator invitationGenerator)
    {
        _logger = logger;
        _users = users;
    }
   
    [HttpGet]
    public async Task<IActionResult> AllUsers()
    {
        try
        {
            _logger.LogInformation("Getting all users by administrator");
            var read = await _users.GetAllUsersAsync();
        
            return Ok(BuildUserDtos(read));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, e.Message);
        }

    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting user {id} by administrator");
            var read = await _users.GetUserById(id);
            return Ok(BuildUserDtos([read]).First());
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("user/{id:guid}/setpassword/{password}")]
    public async Task<IActionResult> SetNewPassword(Guid id, string password)
    {
        try
        {
            await _users.SetPasswordAsync(id, password);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    private List<UserResponse> BuildUserDtos(IEnumerable<User> users) => users.Select(user => new UserResponse
    {
        Id = user.Id,
        Username = user.Username,
        Description = user.Description,
        WasOnline = user.WasOnline,
        IconId = user.IconId,
        PasswordHash = user.PasswordHash,
        InviteId = user.InviteId,
        CreatedOn = user.CreatedOn,
        IsAdmin = user.Invite?.IsAdmin ?? false,
    }).ToList();

}