using AutoMapper;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

            var result = await _messagesLoader.LoadMessagesInChatGroup(
                groupId,
                _currentUser.GetCurrentUserId(),
                query.StartMessageId,
                query.Before,
                query.After);

            var response = _mapper.Map<List<MessageResponse>>(result);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Forbid(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
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
            
            var result = await _messagesLoader.LoadMessagesInUserChat(
                userId,
                _currentUser.GetCurrentUserId(),
                query.StartMessageId,
                query.Before,
                query.After);

            var response = _mapper.Map<List<MessageResponse>>(result);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Forbid(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }

}