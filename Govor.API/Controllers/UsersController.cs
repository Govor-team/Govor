using Govor.API.Services.AdminsStuff.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;


[ApiController] 
[Route("api/admin/[controller]")]
//[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUsersAdministration _users;

    public UsersController(ILogger<UsersController> logger, IUsersAdministration users)
    {
        _logger = logger;
        _users = users;
    }
    
    [HttpGet]
    public async Task<IActionResult> AllUsers()
    {
        _logger.LogInformation("Getting all users by administrator");
        var read = await _users.GetAllUsersAsync();
        
        return Ok(read);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return Ok(id);
    }
}