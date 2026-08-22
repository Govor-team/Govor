using AutoMapper;
using Govor.API.Common.Extensions;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Messages;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRes;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin, User")]
[Route("api")]
public class ChatLoadController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ChatLoadController> _logger;
    private readonly IMessagesLoader _messagesLoader;
    private readonly IMapper _mapper;
    
    public ChatLoadController(
        ILogger<ChatLoadController> logger,
        IMessagesLoader messagesLoader,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _logger = logger;
        _messagesLoader = messagesLoader;
        _currentUser = currentUser;
        _mapper = mapper;
    }
    
    [HttpGet("groups/{groupId:guid}/messages")]
    public async Task<IActionResult> GetGroupMessages( 
         Guid groupId,
         [FromQuery] MessageQuery query)
    {
        try
        {
            if (query.Before < 0 || query.After < 0 || query.After + query.Before > 100)
                return BadRequest("Values must be non-negative and total must not exceed 100.");

            var result = (await _messagesLoader.LoadMessagesInChatGroup(
                groupId,
                _currentUser.GetCurrentUserId(),
                query.StartMessageId,
                query.Before,
                query.After)).Map(messages => _mapper.Map<List<MessageResponse>>(messages));
            
            return result.ToActionResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
    
    [HttpGet("user/{userId:guid}/messages")]
    public async Task<IActionResult> GetUserMessages(
        Guid userId,
        [FromQuery] MessageQuery query)
    {
        try
        {
            if (query.Before < 0 || query.After < 0 || query.After + query.Before > 100)
                return BadRequest("Values must be non-negative and total must not exceed 100.");
            
            var result = (await _messagesLoader.LoadMessagesInUserChat(
                    userId,
                    _currentUser.GetCurrentUserId(),
                    query.StartMessageId,
                    query.Before,
                    query.After)
                ).Map(messages => _mapper.Map<List<MessageResponse>>(messages));

            return result.ToActionResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
}