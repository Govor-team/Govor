using Govor.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("invite")]
public class InviteController : ControllerBase
{
    private readonly IGroupService _groupService;

    public InviteController(IGroupService groupService)
    {
        _groupService = groupService;
    }
    
    [HttpGet("{code}")]
    [Authorize]
    public IActionResult JoinGroup(string code)
    {
        var group = _groupService.GetGroupByInvite(code);
        if (group == null) return NotFound();

        // Вернуть инфу, которая откроется в MAUI-клиенте
        return Ok(new
        {
            groupId = group.Id,
            name = group.Name,
            isChannel = group.IsChannel
        });
    }
}
