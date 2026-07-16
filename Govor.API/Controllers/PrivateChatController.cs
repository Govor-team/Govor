using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Friends;
using Govor.Application.Infrastructure.AdminsStuff;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.PrivateUserChats;
using Govor.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin, User")]
[Route("api")]
public class PrivateChatController : Controller
{
    private readonly ICurrentUserService _currentUser;
    private readonly IVerifyFriendship _verifyFriendship;
    private readonly IUserPrivateChatsCreator _userPrivateChatsCreator;
    private readonly IUserPrivateChatsGetterService _privateChatsGetter;
    private readonly ILogger<ChatLoadController> _logger;
    
    public PrivateChatController(
        ICurrentUserService currentUser, 
        IVerifyFriendship verifyFriendship, 
        IUserPrivateChatsCreator userPrivateChatsCreator,
        IUserPrivateChatsGetterService userPrivateChatsGetterService,
        ILogger<ChatLoadController> logger)
    {
        _currentUser = currentUser;
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

            await _verifyFriendship.VerifyAsync(currentId, friendId);

            var chat = await _userPrivateChatsCreator.CreateAsync(currentId, friendId);
            return Ok(chat.Id);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FriendshipException ex)
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