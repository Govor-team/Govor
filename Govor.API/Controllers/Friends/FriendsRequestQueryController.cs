using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Friends;

[ApiController]
[Authorize]
[Route("api/friends")]
public class FriendsRequestQueryController : Controller
{
    
}