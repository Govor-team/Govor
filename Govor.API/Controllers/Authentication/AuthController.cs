using Govor.Application.Exceptions.AuthService;
using Govor.Application.Exceptions.InvitesService;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.UserSession;
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
    
    //[RequireHttps] 
    [HttpPost("register")]// api/auth/register
    public async Task<IActionResult> Register([FromBody] RegistrationRequest registrationRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invite = await _invitesService.ValidateAsync(registrationRequest.InviteLink);

            var user = await _accountService.RegistrationAsync(registrationRequest.Name, registrationRequest.Password,
                invite);
            
            _logger.LogInformation($"Register request for {user.Username} with id {user.Id} processed successfully");

            var tokens = await  _userSession.OpenSessionAsync(user, registrationRequest.DeviceInfo);
            
            _logger.LogInformation($"Session for user {user.Username} with id {user.Id} has been opened");
            
            return Ok(tokens);
        }
        catch (UserAlreadyExistException ex)
        {
            _logger.LogWarning(ex, $"Registration failed for user {registrationRequest.Name}");
            return BadRequest("Registration failed: user already exists.");
        }
        catch (InviteLinkInvalidException ex)
        {
            _logger.LogWarning(ex, $"Invite link invalid: {registrationRequest.InviteLink}");
            return BadRequest("Invite link invalid.");
        }
        catch (InvalidUsernameException ex)
        {
            _logger.LogWarning(ex, $"Invalid username: {registrationRequest.Name}");
            return BadRequest($"Invalid username: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for user {Name}", registrationRequest.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
    
    //[RequireHttps] 
    [HttpPost("login")]// api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _accountService.LoginAsync(loginRequest.Name, loginRequest.Password);
            _logger.LogInformation($"Login request for {user.Username} with id {user.Id} processed successfully");
            
            var tokens = await  _userSession.OpenSessionAsync(user, loginRequest.DeviceInfo);
            
            _logger.LogInformation($"Session for user {user.Username} with id {user.Id} has been opened");
            
            return Ok(tokens);
        }
        catch (UserNotRegisteredException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {Name}", loginRequest.Name);
            return BadRequest("Login failed: user does not exist.");
        }
        catch (LoginUserException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {Name}", loginRequest.Name);
            return BadRequest("Login failed: username or password is incorrect.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Name}", loginRequest.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}