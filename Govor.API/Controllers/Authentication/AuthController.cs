using Govor.Application.Authentication;
using Govor.Application.Authentication.Exceptions;
using Govor.Application.Users.UserSessions;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Register([FromBody] RegistrationRequest registrationRequest)
    {
       
        var inviteResult = await _invitesService.ValidateAsync(registrationRequest.InviteLink);
        if (!inviteResult.IsSuccess)
        {
            _logger.LogWarning("Invite link invalid: {InviteLink}. Error: {Error}", registrationRequest.InviteLink,
                inviteResult.Error);
            return BadRequest($"Invite link invalid: {inviteResult.Error.Message}");
        }
        
        var userResult = await _accountService.RegistrationAsync(
            registrationRequest.Name,
            registrationRequest.Password,
            inviteResult.Value);

        if (userResult.IsFailure)
        {
            _logger.LogWarning("Registration failed for user {Name}. Error: {Error}", registrationRequest.Name,
                userResult.Error);
            
            return userResult.Error.Code switch
            {
                nameof(UserAlreadyExistException) => BadRequest($"Registration failed: {userResult.Error.Message}"),
                nameof(InvalidUsernameException) => BadRequest($"Invalid username: {userResult.Error.Message}"),
                _ => BadRequest($"Registration failed: {userResult.Error.Message}")
            };
        }

        var user = userResult.Value;
        _logger.LogInformation("Register request for {Username} with id {Id} processed successfully", user.Username,
            user.Id);
        
        var sessionResult = await _userSession.OpenSessionAsync(user, registrationRequest.DeviceInfo);
        if (sessionResult.IsFailure)
        {
            _logger.LogError("Failed to open session for user {Username}. Error: {Error}", user.Username,
                sessionResult.Error.Message);
            return StatusCode(500, "An error occurred while creating the session.");
        }

        _logger.LogInformation("Session for user {Username} with id {Id} has been opened", user.Username, user.Id);
        return Ok(sessionResult.Value); 
    }

    
    [HttpPost("login")] // api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var userResult = await _accountService.LoginAsync(loginRequest.Name, loginRequest.Password);
    
        if (userResult.IsFailure)
        {
            _logger.LogWarning("Login failed for user {Name}. Error: {Code}", loginRequest.Name, userResult.Error);
            
            return userResult.Error.Code switch
            {
                nameof(UserNotRegisteredException) => BadRequest("Login failed: user does not exist."),
                nameof(InvalidOperationException) => BadRequest("Login failed: username or password is incorrect."),
                _ => BadRequest($"Login failed: {userResult.Error.Message}")
            };
        }
        
        var user = userResult.Value; 
        _logger.LogInformation("Login request for {Username} with id {Id} processed successfully", user.Username, user.Id);
        
        var sessionResult = await _userSession.OpenSessionAsync(user, loginRequest.DeviceInfo);
    
        if (sessionResult.IsFailure)
        {
            _logger.LogError("Failed to open session for user {Username}. Error: {Error}", user.Username, sessionResult.Error);
            return StatusCode(500, "An error occurred while creating the session.");
        }
    
        _logger.LogInformation("Session for user {Username} with id {Id} has been opened", user.Username, user.Id);
    
        return Ok(sessionResult.Value);
    }
}