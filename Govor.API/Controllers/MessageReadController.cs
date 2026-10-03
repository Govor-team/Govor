using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Messages;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses.SignalR;
using Govor.Domain.Models.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/chats/{chatId:guid}")]
public class MessageReadController : ControllerBase
{
    private readonly IMessageReadingService _reader;
    private readonly ICurrentUserService _currentUser;
    private readonly IChatNotificationService _notifier;
    private readonly IMapper _mapper;

    public MessageReadController(IMessageReadingService reader, ICurrentUserService currentUser,
        IChatNotificationService notifier, IMapper mapper)
    {
        _reader = reader;
        _currentUser = currentUser;
        _notifier = notifier;
        _mapper = mapper;
    }

    [HttpPost("read")]
    public async Task<IActionResult> Read(Guid chatId, [FromBody] ReadChatRequest request)
    {
        var result = await _reader.ReadChatAsync(_currentUser.GetCurrentUserId(), chatId,
            request.RecipientType, request.MessageIds, request.UpToMessageId);
        if (result.IsFailure)
            return result.ToActionResult();

        var response = _mapper.Map<ChatReadResponse>(result.Value);
        await _notifier.NotifyChatReadAsync(response);
        return Ok(response);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid chatId, [FromQuery] RecipientType recipientType)
    {
        var result = await _reader.GetUnreadCountAsync(_currentUser.GetCurrentUserId(), chatId, recipientType);
        return result.IsFailure ? result.ToActionResult() : Ok(new { unreadCount = result.Value });
    }
}
