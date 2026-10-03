using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Reactions;
using Govor.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/reaction-packs")]
public class ReactionPacksController(IReactionPackService packs, ICurrentUserService currentUser, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery, Range(0, int.MaxValue)] int skip = 0,
        [FromQuery, Range(1, 100)] int take = 50) =>
        Ok(mapper.Map<List<ReactionPackResponse>>(await packs.ListAsync(currentUser.GetCurrentUserId(), skip: skip, take: take)));

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery, Range(0, int.MaxValue)] int skip = 0,
        [FromQuery, Range(1, 100)] int take = 50) =>
        Ok(mapper.Map<List<ReactionPackResponse>>(await packs.ListAsync(currentUser.GetCurrentUserId(), installedOnly: true, skip: skip, take: take)));

    [HttpGet("{packId:guid}")]
    public async Task<IActionResult> Get(Guid packId) => (await packs.GetAsync(packId))
        .Map(p => mapper.Map<ReactionPackResponse>(p)).ToActionResult();

    [HttpGet("shared/{shareCode}")]
    public async Task<IActionResult> Shared(string shareCode) => (await packs.GetSharedAsync(shareCode))
        .Map(p => mapper.Map<ReactionPackResponse>(p)).ToActionResult();

    [HttpGet("/api/reactions/{reactionId:guid}")]
    public async Task<IActionResult> Reaction(Guid reactionId) => (await packs.GetReactionAsync(reactionId))
        .Map(r => mapper.Map<ReactionItemResponse>(r)).ToActionResult();

    [HttpPut("{packId:guid}/subscription")]
    public async Task<IActionResult> Subscribe(Guid packId) =>
        (await packs.SubscribeAsync(currentUser.GetCurrentUserId(), packId, true)).ToActionResult();

    [HttpDelete("{packId:guid}/subscription")]
    public async Task<IActionResult> Unsubscribe(Guid packId) =>
        (await packs.SubscribeAsync(currentUser.GetCurrentUserId(), packId, false)).ToActionResult();

    [HttpGet("limits")]
    public IActionResult Limits() => Ok(new
    {
        maxDimension = ReactionMediaLimits.MaxDimension, maxUploadBytes = ReactionMediaLimits.MaxUploadBytes,
        maxImageBytes = ReactionMediaLimits.MaxImageBytes, maxGifBytes = ReactionMediaLimits.MaxGifBytes,
        maxFrames = ReactionMediaLimits.MaxFrames, maxDurationMilliseconds = ReactionMediaLimits.MaxDurationMilliseconds,
        maxItemsPerPack = 100
    });
}
