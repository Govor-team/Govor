using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Reactions;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reaction-packs")]
public class ReactionPacksAdminController(IReactionPackService packs, ICurrentUserService currentUser, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery, Range(0, int.MaxValue)] int skip = 0,
        [FromQuery, Range(1, 100)] int take = 50) =>
        Ok(mapper.Map<List<ReactionPackResponse>>(await packs.ListAsync(currentUser.GetCurrentUserId(), includeDisabled: true, skip: skip, take: take)));

    [HttpGet("{packId:guid}")]
    public async Task<IActionResult> Get(Guid packId) => (await packs.GetAsync(packId, includeDisabled: true))
        .Map(p => mapper.Map<ReactionPackResponse>(p)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReactionPackRequest request) =>
        (await packs.CreateAsync(currentUser.GetCurrentUserId(), request.Name, request.Description))
        .Map(p => mapper.Map<ReactionPackResponse>(p)).ToActionResult();

    [HttpPut("{packId:guid}")]
    public async Task<IActionResult> Update(Guid packId, [FromBody] UpdateReactionPackRequest request) =>
        (await packs.UpdateAsync(currentUser.GetCurrentUserId(), packId, request.Name, request.Description, request.IsEnabled))
        .Map(p => mapper.Map<ReactionPackResponse>(p)).ToActionResult();

    [HttpDelete("{packId:guid}")]
    public async Task<IActionResult> Disable(Guid packId) => (await packs.DisableAsync(currentUser.GetCurrentUserId(), packId)).ToActionResult();

    [HttpPost("{packId:guid}/reactions/emoji")]
    public async Task<IActionResult> Emoji(Guid packId, [FromBody] CreateEmojiReactionRequest request) =>
        (await packs.AddEmojiAsync(currentUser.GetCurrentUserId(), packId, request.Name, request.Emoji))
        .Map(r => mapper.Map<ReactionItemResponse>(r)).ToActionResult();

    [HttpPost("{packId:guid}/reactions/media")]
    [RequestSizeLimit(2_200_000)]
    public async Task<IActionResult> Media(Guid packId, [FromForm] ReactionUploadRequest request)
    {
        if (request.File is null || request.File.Length == 0 || request.File.Length > ReactionMediaLimits.MaxUploadBytes)
            return BadRequest("Upload must contain at most 2 MiB.");
        using var buffer = new MemoryStream();
        await request.File.CopyToAsync(buffer, HttpContext.RequestAborted);
        return (await packs.AddMediaAsync(currentUser.GetCurrentUserId(), packId, request.Name,
            buffer.ToArray(), HttpContext.RequestAborted)).Map(r => mapper.Map<ReactionItemResponse>(r)).ToActionResult();
    }

    [HttpDelete("{packId:guid}/reactions/{reactionId:guid}")]
    public async Task<IActionResult> DisableReaction(Guid packId, Guid reactionId) =>
        (await packs.DisableReactionAsync(currentUser.GetCurrentUserId(), packId, reactionId)).ToActionResult();
}

public class ReactionUploadRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] public IFormFile File { get; set; }
}
