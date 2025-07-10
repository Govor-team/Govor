using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Friends;

[ApiController]
[Authorize]
[Route("api/friends")]
public class FriendsRequestQueryController : Controller
{
    private readonly ILogger<FriendsController> _logger;
    private readonly IFriendRequestQueryService _friendsService;
    private readonly ICurrentUserService _currentUserService;

    public FriendsRequestQueryController(ILogger<FriendsController> logger,
        IFriendRequestQueryService friendsService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _friendsService = friendsService;
        _currentUserService = currentUserService;
    }
    

}