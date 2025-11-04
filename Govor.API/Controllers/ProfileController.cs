using AutoMapper;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Contracts.DTOs;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize(Roles = "Admin,User")]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IMediaService _mediaService;
    private readonly IProfileService _profileService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public ProfileController(
        ILogger<ProfileController> logger,
        IMediaService mediaService,
        IProfileService profileService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _logger = logger;
        _mediaService = mediaService;
        _profileService = profileService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    [HttpGet("dowload")]
    public async Task<IActionResult> DowloadProfile()
    {
        try
        {
            var userId = _currentUserService.GetCurrentUserId();
            var user = await _profileService.GetUserProfileAsync(userId);

            if (user is null)
                return NotFound("Profile not found");

            var dto = _mapper.Map<UserProfileDto>(user);
            return Ok(dto);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, "Unexpected Error! Please try again later.");
        }
    }
}
