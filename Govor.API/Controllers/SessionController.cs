using AutoMapper;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.UserSession;
using Govor.Contracts.DTOs;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,User")]
[Route("api/[controller]")]
public class SessionController : Controller
{
    private readonly ILogger<SessionController> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentUserSessionService _currentUserSessionService;
    private readonly IUserSessionReader _userSessionReader;
    private readonly IUserSessionRevoker _userSessionRevoker;
    private readonly IMapper _mapper;

    public SessionController(
        ILogger<SessionController> logger,
        ICurrentUserService currentUserService,
        ICurrentUserSessionService currentUserSessionService,
        IUserSessionReader userSessionReader,
        IUserSessionRevoker userSessionRevoker,
        IMapper mapper)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _currentUserSessionService = currentUserSessionService;
        _userSessionReader = userSessionReader;
        _userSessionRevoker = userSessionRevoker;
        _mapper = mapper;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllSessions()
    {
        try
        {
            var sessions = await _userSessionReader.GetAllSessionsAsync(_currentUserService.GetCurrentUserId());
            return Ok(_mapper.Map<List<SessionDto>>(sessions));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
    
    [HttpDelete("close/{sessionId}")]
    public async Task<IActionResult> CloseSession(Guid sessionId)
    {
        try
        {
            if (sessionId == Guid.Empty)
                return BadRequest("Invalid sessionId.");
            
            await _userSessionRevoker.CloseSessionByIdAsync(sessionId,
                _currentUserService.GetCurrentUserId());
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound(ex.Message); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }

    [HttpDelete("close")]
    public async Task<IActionResult> CloseCurrent()
    {
        try
        {
            await _userSessionRevoker.CloseSessionByIdAsync(
                _currentUserSessionService.GetUserSessionId(),
                _currentUserService.GetCurrentUserId());

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }

    [HttpDelete("close/all")]
    public async Task<IActionResult> CloseAllSessions()
    {
        try
        {
            await _userSessionRevoker.CloseAllSessionsAsync(_currentUserService.GetCurrentUserId());
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
}