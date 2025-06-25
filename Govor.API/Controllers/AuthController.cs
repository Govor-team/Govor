using Govor.API.Services.Authentication.Interfaces;
using Govor.Application.Exceptions.AuthService;
using Govor.Application.Exceptions.InvitesService;
using Govor.Application.Interfaces.Authentication;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private IInvitesService _invitesService;
    private IAccountService _accountService;
    private ILogger<AuthController> _logger;
    
    public AuthController(IAccountService accountService, IInvitesService invitesService, ILogger<AuthController> logger)
    {
        _accountService = accountService;
        _invitesService = invitesService;
        _logger = logger;
    }
    
    [HttpPost("register")]// api/auth/register
    [RequireHttps] 
    public async Task<IActionResult> Register([FromBody] RegistrationRequest registrationRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invite = _invitesService.Validate(registrationRequest.InviteLink);

            var token = await _accountService.RegistrationAsync(registrationRequest.Name, registrationRequest.Password, invite);
            _logger.LogInformation($"Register request for {registrationRequest.Name}");
            return Ok(new { token });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for user {Name}", registrationRequest.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
    
    [HttpPost("login")]// api/auth/login
    [RequireHttps] 
    public async Task<IActionResult> Login([FromBody] LoginRequest userRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _accountService.LoginAsync(userRequest.Name, userRequest.Password);
            _logger.LogInformation($"Login request for {userRequest.Name}");
            return Ok(new { token });
        }
        catch (UserNotRegisteredException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {Name}", userRequest.Name);
            return BadRequest("Login failed: user does not exist.");
        }
        catch (LoginUserException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {Name}", userRequest.Name);
            return BadRequest("Login failed: username or password is incorrect.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Name}", userRequest.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}