using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.DTOs;
using Govor.Core.Models;
using Govor.Core.Repositories.PrivateChats;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin, User")]
[Route("api")]
public class PrivateChatController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUsersRepository _usersRepository; 
    private readonly IVerifyFriendship _verifyFriendship;
    private readonly IUserPrivateChatsCreator _userPrivateChatsCreator;
    private readonly IUserPrivateChatsGetterService _privateChatsGetter;
    private readonly ILogger<ChatLoadController> _logger;
    
    public PrivateChatController(
        ICurrentUserService currentUser, 
        IUsersRepository usersRepository,
        IVerifyFriendship verifyFriendship, 
        IPrivateChatsRepository privateChats,
        IUserPrivateChatsCreator userPrivateChatsCreator,
        IUserPrivateChatsGetterService userPrivateChatsGetterService,
        ILogger<ChatLoadController> logger)
    {
        _currentUser = currentUser;
        _usersRepository = usersRepository;
        _verifyFriendship = verifyFriendship;
        _userPrivateChatsCreator = userPrivateChatsCreator;
        _privateChatsGetter = userPrivateChatsGetterService;
        _logger = logger;
    }
    
    [HttpGet("user/{friendId:guid}/private-chat")]
    public async Task<IActionResult> GetChatByFriendId(Guid friendId)
    {
        try
        {
            var currentId = _currentUser.GetCurrentUserId();

            if (!await _usersRepository.ExistsByIdAsync(friendId))
            {
                _logger.LogWarning("User not exist {0}", friendId);
                return NotFound("User not exist.");
            }

            await _verifyFriendship.VerifyAsync(currentId, friendId);
            
            var chat = await _userPrivateChatsCreator.CreateAsync(currentId, friendId);
            return Ok(chat.Id);
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

    [HttpGet("user/private-chats")]
    public async Task<IActionResult> GetChatsByFriends()
    {
        try
        {
            var currentId = _currentUser.GetCurrentUserId();
            var chats = await _privateChatsGetter.GetUserChatsAsync(currentId);

            var result = chats.Select(chat => new PrivateChatDto
            {
                ChatId = chat.Id,
                FriendId = chat.UserAId == currentId ? chat.UserBId : chat.UserAId
            }).ToList();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
}