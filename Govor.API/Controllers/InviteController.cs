using Govor.Application.Groups;
using Govor.Application.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("invite")]
public class InviteController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<InviteController> _logger;
    private readonly ICurrentUserService _currentUser;
    public InviteController(IGroupService groupService, 
        ILogger<InviteController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }
    
    [Authorize]
    [HttpGet("{code}")]
    public IActionResult JoinGroup(string code)
    {
        try
        {
            _groupService.AddUserToGroupByInvitationAsync(_currentUser.GetCurrentUserId(), code);
            
            var group = _groupService.GetGroupByInviteCode(code);

            return Ok(new
            {
                groupId = group.Id,
                name = group.Name,
                isChannel = group.IsChannel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ex.Message);
        }
    }
}
