using Govor.API.Common.Extensions;
using Govor.Application.Authentication;
using Govor.Application.Users.UserSessions;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartRes;

namespace Govor.API.Controllers.Authentication;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private IUserSessionOpener _userSession;
    private IInvitesService _invitesService;
    private IAccountService _accountService;
    private ILogger<AuthController> _logger;
    
    public AuthController(
        IAccountService accountService,
        IInvitesService invitesService,
        IUserSessionOpener userSessionOpener,
        ILogger<AuthController> logger)
    {
        _userSession = userSessionOpener;
        _accountService = accountService;
        _invitesService = invitesService;
        _logger = logger;
    }

    [HttpPost("register")] // api/auth/register
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        _logger.LogInformation("Processing registration request for: {Name}", request.Name);
        
        var result = await _invitesService.ValidateAsync(request.InviteLink)
            .BindAsync(invite => _accountService.RegistrationAsync(request.Name, request.Password, invite))
            .TapAsync(user => _logger.LogInformation("User {Username} ({Id}) registered successfully", user.Username, user.Id))
            .BindAsync(user => _userSession.OpenSessionAsync(user, request.DeviceInfo));
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Registration pipeline failed. Error: {Code} - {Message}", 
                result.Error.Code, result.Error.Message);
        }
        else
        {
            _logger.LogInformation("Session opened successfully for the request.");
        }
        
        return result.ToActionResult();
    }
    
    [HttpPost("login")] // api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Processing registration request for: {Name}", request.Name);
        
        var result = await _accountService.LoginAsync(request.Name, request.Password)
            .TapAsync(user => _logger.LogInformation("User {Username} ({Id}) logged in.", user.Username, user.Id))
            .BindAsync(user => _userSession.OpenSessionAsync(user, request.DeviceInfo));
    
        if (result.IsFailure)
        {
            _logger.LogWarning("Login pipeline failed. Error: {Code} - {Message}", 
                result.Error.Code, result.Error.Message);
        }
        else
        {
            _logger.LogInformation("Session opened successfully for the request.");
        }
        
        return result.ToActionResult();
    }
}