using AutoMapper;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chats")]
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
    
    [HttpGet("group-messages")]
    public async Task<IActionResult> GetChatMessages( 
        [FromQuery] Guid chatId,
        [FromQuery] Guid? startMessageId,
        [FromQuery] int pageSize)
    {
        try
        {
            var result = await _messagesLoader.LoadLastMessagesInChatGroup(
                chatId,
                _currentUser.GetCurrentUserId(),
                startMessageId,
                pageSize);

            var response = _mapper.Map<List<MessageResponse>>(result);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
    
    [HttpGet("user-messages")]
    public async Task<IActionResult> GetUserMessages(
        [FromQuery] Guid userId,
        [FromQuery] Guid? startMessageId,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _messagesLoader.LoadLastMessagesInUserChat(
                userId,
                _currentUser.GetCurrentUserId(),
                startMessageId,
                pageSize);

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
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }

}