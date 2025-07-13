using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Contracts.Requests;
using Govor.Core.Models;
using Govor.Core.Repositories.MediasAttachments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = "User,Admin")]
public class MediaController : Controller
{
    private readonly ILogger<MediaController> _logger;
    private readonly IMediaService _mediaService;
    private readonly ICurrentUserService _currentUserService;

    public MediaController(
        ILogger<MediaController> logger,
        IMediaService mediaService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _mediaService = mediaService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)] // ~100MB
    public async Task<IActionResult> Upload([FromForm] MediaUploadRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Чтение байт из IFormFile
            using var memoryStream = new MemoryStream();
            await request.Data.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            var media = new Media(
                _currentUserService.GetCurrentUserId(),
                DateTime.UtcNow,
                fileBytes,
                request.FileName,
                request.Type,
                request.MimeType,
                request.EncryptedKey
            );

            var result = await _mediaService.UploadMediaAsync(media);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var media = await _mediaService.GetMediaByIdAsync(id);
            return File(media.Data, media.MineType, media.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media");
            return StatusCode(500, "Internal server error");
        }
    }
}
