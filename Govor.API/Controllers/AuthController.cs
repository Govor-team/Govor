using Govor.API.Services.Authentication;
using Govor.Core.DTOs;
using Govor.API.Services.Authentication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private IAccountService _accountService;
    private ILogger<AuthController> _logger;
    
    public AuthController(IAccountService accountService, ILogger<AuthController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }
    
    [HttpPost("register")]// api/auth/register
    [RequireHttps] 
    public async Task<IActionResult> Register([FromBody] UserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _accountService.RegistrationAsync(userDto.Name, userDto.Password);
            _logger.LogInformation($"Register request for {userDto.Name}");
            return Ok(new { token });
        }
        catch (UserAlreadyExistException ex)
        {
            _logger.LogWarning(ex, $"Registration failed for user {userDto.Name}");
            return BadRequest("Registration failed: user already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for user {Name}", userDto.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
    
    [HttpPost("login")]// api/auth/login
    [RequireHttps] 
    public async Task<IActionResult> Login([FromBody] UserDto userDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _accountService.LoginAsync(userDto.Name, userDto.Password);
            _logger.LogInformation($"Login request for {userDto.Name}");
            return Ok(new { token });
        }
        catch (UserNotRegisteredException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {Name}", userDto.Name);
            return BadRequest("Login failed: user does not exist.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Name}", userDto.Name);
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}