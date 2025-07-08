using Govor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chats")]
public class ChatLoadController : Controller
{
    public ChatLoadController(ILogger<ChatLoadController> logger, IMessagesLoader messagesLoader)
    {
        
    }
}