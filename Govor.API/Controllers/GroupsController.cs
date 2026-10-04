using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Groups;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Messages;
using Govor.Application.Messages.Parameters;
using Govor.Application.Reactions;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Govor.Contracts.Responses.SignalR;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRes;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/groups")]
public class GroupsController(IGroupManagementService groups, ICurrentUserService currentUser, IMapper mapper,
    GovorDbContext context, IMessageRemovingService remover, IChatNotificationService notifier, IGroupAvatarService avatars) : ControllerBase
{
    private Guid ActorId => currentUser.GetCurrentUserId();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
    {
        var result = await groups.CreateAsync(ActorId, request.Name, request.Description, request.IsPrivate, request.IsChannel);
        if (result.IsSuccess) await notifier.NotifyGroupMemberChangedAsync(result.Value.Group.Id, ActorId);
        return result.Map(g => mapper.Map<GroupResponse>(g)).ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery, MaxLength(100)] string q = "",
        [FromQuery, Range(0, int.MaxValue)] int skip = 0, [FromQuery, Range(1, 100)] int take = 50) =>
        Ok(mapper.Map<List<GroupResponse>>(await groups.SearchAsync(ActorId, q, skip, take)));

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery, Range(0, int.MaxValue)] int skip = 0,
        [FromQuery, Range(1, 100)] int take = 50) => Ok(mapper.Map<List<GroupResponse>>(await groups.MineAsync(ActorId, skip, take)));

    [HttpGet("{groupId:guid}")]
    public async Task<IActionResult> Get(Guid groupId) => (await groups.GetAsync(ActorId, groupId))
        .Map(g => mapper.Map<GroupResponse>(g)).ToActionResult();

    [HttpPut("{groupId:guid}")]
    public async Task<IActionResult> Update(Guid groupId, [FromBody] UpdateGroupRequest request)
    {
        var result = await groups.UpdateAsync(ActorId, groupId, request.Name, request.Description, request.IsPrivate);
        if (result.IsSuccess) await notifier.NotifyGroupProfileChangedAsync(groupId);
        return result.Map(g => mapper.Map<GroupResponse>(g)).ToActionResult();
    }

    [HttpPost("{groupId:guid}/join")]
    public async Task<IActionResult> Join(Guid groupId)
    {
        var result = await groups.JoinPublicAsync(ActorId, groupId);
        if (result.IsSuccess) await notifier.NotifyGroupMemberChangedAsync(groupId, ActorId);
        return result.Map(g => mapper.Map<GroupResponse>(g)).ToActionResult();
    }

    [HttpPost("{groupId:guid}/avatar")]
    [RequestSizeLimit(2_200_000)]
    public async Task<IActionResult> UploadAvatar(Guid groupId, [FromForm] GroupAvatarUploadRequest request)
    {
        if (request.File.Length == 0 || request.File.Length > ReactionMediaLimits.MaxUploadBytes)
            return BadRequest("Upload must contain at most 2 MiB.");
        using var buffer = new MemoryStream();
        await request.File.CopyToAsync(buffer, HttpContext.RequestAborted);
        var result = await avatars.UploadAsync(ActorId, groupId, buffer.ToArray(), HttpContext.RequestAborted);
        if (result.IsSuccess) await notifier.NotifyGroupProfileChangedAsync(groupId);
        return result.Map(id => new { imageId = id, mediaUrl = $"/api/media/download/{id}" }).ToActionResult();
    }

    [HttpDelete("{groupId:guid}/avatar")]
    public async Task<IActionResult> RemoveAvatar(Guid groupId)
    {
        var result = await avatars.RemoveAsync(ActorId, groupId);
        if (result.IsSuccess) await notifier.NotifyGroupProfileChangedAsync(groupId);
        return result.ToActionResult();
    }

    [HttpDelete("{groupId:guid}/members/me")]
    public Task<IActionResult> Leave(Guid groupId) => ChangeMemberAsync(groupId, ActorId, groups.LeaveAsync(ActorId, groupId));

    [HttpGet("{groupId:guid}/members")]
    public async Task<IActionResult> Members(Guid groupId, [FromQuery] bool bannedOnly = false,
        [FromQuery, Range(0, int.MaxValue)] int skip = 0, [FromQuery, Range(1, 100)] int take = 50) =>
        (await groups.MembersAsync(ActorId, groupId, bannedOnly, skip, take))
        .Map(m => mapper.Map<List<GroupMemberResponse>>(m)).ToActionResult();

    [HttpPut("{groupId:guid}/members/{userId:guid}/administrator")]
    public Task<IActionResult> Promote(Guid groupId, Guid userId) => ChangeMemberAsync(groupId, userId, groups.SetAdministratorAsync(ActorId, groupId, userId, true));

    [HttpDelete("{groupId:guid}/members/{userId:guid}/administrator")]
    public Task<IActionResult> Demote(Guid groupId, Guid userId) => ChangeMemberAsync(groupId, userId, groups.SetAdministratorAsync(ActorId, groupId, userId, false));

    [HttpPut("{groupId:guid}/members/{userId:guid}/ban")]
    public Task<IActionResult> Ban(Guid groupId, Guid userId) => ChangeMemberAsync(groupId, userId, groups.SetBanAsync(ActorId, groupId, userId, true));

    [HttpDelete("{groupId:guid}/members/{userId:guid}/ban")]
    public Task<IActionResult> Unban(Guid groupId, Guid userId) => ChangeMemberAsync(groupId, userId, groups.SetBanAsync(ActorId, groupId, userId, false));

    [HttpPost("{groupId:guid}/ownership/{userId:guid}")]
    public async Task<IActionResult> Transfer(Guid groupId, Guid userId)
    {
        var result = await groups.TransferOwnershipAsync(ActorId, groupId, userId);
        if (result.IsSuccess)
        {
            await notifier.NotifyGroupProfileChangedAsync(groupId);
            await notifier.NotifyGroupMemberChangedAsync(groupId, ActorId);
            await notifier.NotifyGroupMemberChangedAsync(groupId, userId);
        }
        return result.ToActionResult();
    }

    private async Task<IActionResult> ChangeMemberAsync(Guid groupId, Guid userId, Task<Result<Unit, Error>> mutation)
    {
        var result = await mutation;
        if (result.IsSuccess) await notifier.NotifyGroupMemberChangedAsync(groupId, userId);
        return result.ToActionResult();
    }

    [HttpPost("{groupId:guid}/invitations")]
    public async Task<IActionResult> CreateInvitation(Guid groupId, [FromBody] CreateGroupInvitationRequest request) =>
        (await groups.CreateInvitationAsync(ActorId, groupId, request.ValidForDays, request.MaxParticipants, request.Description))
        .Map(i => mapper.Map<GroupInvitationResponse>(i)).ToActionResult();

    [HttpGet("{groupId:guid}/invitations")]
    public async Task<IActionResult> Invitations(Guid groupId) => (await groups.InvitationsAsync(ActorId, groupId))
        .Map(i => mapper.Map<List<GroupInvitationResponse>>(i)).ToActionResult();

    [HttpDelete("{groupId:guid}/invitations/{invitationId:guid}")]
    public async Task<IActionResult> RevokeInvitation(Guid groupId, Guid invitationId) =>
        (await groups.RevokeInvitationAsync(ActorId, groupId, invitationId)).ToActionResult();

    [HttpDelete("{groupId:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> RemoveMessage(Guid groupId, Guid messageId)
    {
        if (!await context.Messages.AnyAsync(m => m.Id == messageId && m.RecipientId == groupId && m.RecipientType == RecipientType.Group))
            return NotFound();
        var result = await remover.DeleteMessageAsync(new DeleteMessage(ActorId, messageId, ForceRemove: true));
        if (result.IsFailure) return result.ToActionResult();
        var message = result.Value;
        await notifier.NotifyMessageRemovedAsync(new MessageRemovedResponse
        {
            MessageId = message.Id, SenderId = message.SenderId, RecipientId = groupId,
            RecipientType = RecipientType.Group, RequestType = Govor.Contracts.Requests.SignalR.RemoveMessageRequestType.ForceRemove
        });
        return NoContent();
    }
}

public class GroupAvatarUploadRequest
{
    [Required] public IFormFile File { get; set; }
}
