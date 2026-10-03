using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Reactions;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/messages/{messageId:guid}/reactions")]
public class MessageReactionsController(IMessageReactionService reactions, ICurrentUserService currentUser,
    IMapper mapper, IChatNotificationService notifier) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid messageId) => (await reactions.GetAsync(currentUser.GetCurrentUserId(), messageId))
        .Map(s => mapper.Map<MessageReactionsChangedResponse>(s)).ToActionResult();

    [HttpPut("me")]
    public Task<IActionResult> Set(Guid messageId, [FromBody] SetReactionRequest request) => ChangeAsync(messageId, request.ReactionId);

    [HttpDelete("me")]
    public Task<IActionResult> Remove(Guid messageId) => ChangeAsync(messageId, null);

    private async Task<IActionResult> ChangeAsync(Guid messageId, Guid? reactionId)
    {
        var result = await reactions.SetAsync(currentUser.GetCurrentUserId(), messageId, reactionId);
        if (result.IsFailure) return result.ToActionResult();
        var response = mapper.Map<MessageReactionsChangedResponse>(result.Value);
        if (result.Value.Changed) await notifier.NotifyMessageReactionsChangedAsync(response);
        return Ok(response);
    }
}
