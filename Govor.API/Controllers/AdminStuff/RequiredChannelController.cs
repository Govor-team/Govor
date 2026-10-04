using Govor.API.Common.Extensions;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Groups;
using Govor.Application.Infrastructure.Extensions;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/required-channel")]
public class RequiredChannelController(IGroupManagementService groups, ICurrentUserService currentUser,
    IChatNotificationService notifier) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => (await groups.GetRequiredChannelAsync(currentUser.GetCurrentUserId()))
        .Map(s => new { channelId = s.RequiredChannelId, allowLeave = s.AllowLeave }).ToActionResult();

    [HttpPut]
    public async Task<IActionResult> Set([FromBody] RequiredChannelRequest request)
    {
        var actorId = currentUser.GetCurrentUserId();
        var previous = await groups.GetRequiredChannelAsync(actorId);
        if (previous.IsFailure) return previous.ToActionResult();
        var result = await groups.SetRequiredChannelAsync(actorId, request.ChannelId, request.AllowLeave);
        if (result.IsSuccess)
            foreach (var channelId in new[] { previous.Value.RequiredChannelId, result.Value.RequiredChannelId }.OfType<Guid>().Distinct())
                await notifier.NotifyGroupProfileChangedAsync(channelId);
        return result.Map(s => new { channelId = s.RequiredChannelId, allowLeave = s.AllowLeave }).ToActionResult();
    }
}
