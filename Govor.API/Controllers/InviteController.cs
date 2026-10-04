using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Groups;
using Govor.Application.Infrastructure.Extensions;
using Govor.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/group-invites")]
public class InviteController(IGroupManagementService groups, ICurrentUserService currentUser, IMapper mapper,
    IChatNotificationService notifier) : ControllerBase
{
    // GET only previews: link scanners/prefetch must not join an account to a group.
    [HttpGet("{code}")]
    [HttpGet("/invite/{code}")]
    public async Task<IActionResult> Preview(string code) => (await groups.PreviewInvitationAsync(currentUser.GetCurrentUserId(), code))
        .Map(g => mapper.Map<GroupResponse>(g)).ToActionResult();

    [HttpPost("{code}/join")]
    [HttpPost("/invite/{code}/join")]
    public async Task<IActionResult> Join(string code)
    {
        var actorId = currentUser.GetCurrentUserId();
        var result = await groups.JoinByInvitationAsync(actorId, code);
        if (result.IsSuccess) await notifier.NotifyGroupMemberChangedAsync(result.Value.Group.Id, actorId);
        return result.Map(g => mapper.Map<GroupResponse>(g)).ToActionResult();
    }
}
