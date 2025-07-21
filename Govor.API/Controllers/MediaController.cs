using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Contracts.Requests;
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
    private readonly IAccesserToDownloadMedia _accesser;
    private readonly ICurrentUserService _currentUserService;

    public MediaController(
        ILogger<MediaController> logger,
        IMediaService mediaService,
        IAccesserToDownloadMedia accesser,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _accesser = accesser;
        _mediaService = mediaService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)] // ~20MB
    public async Task<IActionResult> Upload([FromForm] MediaUploadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request?.FromFile is null || request.FromFile.Length == 0)
            return BadRequest("No file uploaded");

        if (request.FromFile.Length > 20_000_000)
            return BadRequest("File is too large");

        if (string.IsNullOrWhiteSpace(request.MimeType))
            return BadRequest("Missing MIME type");

        try
        {
            byte[] fileBytes = await ReadFileAsync(request.FromFile);

            var media = new Media(
                _currentUserService.GetCurrentUserId(),
                DateTime.UtcNow,
                Path.GetFileName(request.FromFile.FileName),
                fileBytes,
                request.Type,
                request.MimeType,
                request.EncryptedKey
            );

            var result = await _mediaService.UploadMediaAsync(media);

            _logger.LogInformation("Uploaded file {FileName} from user {UserId}",
                media.FileName, media.UploaderId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<byte[]> ReadFileAsync(IFormFile file)
    {
        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }


    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        try
        {
            var userId = _currentUserService.GetCurrentUserId();

            if (!await _accesser.HasAccessAsync(id, userId))
                return Forbid();

            var media = await _mediaService.GetMediaByIdAsync(id);

            return File(media.Data, media.MimeType, Path.GetFileName(media.FileName));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return NotFound("Media not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media");
            return StatusCode(500, "Internal server error");
        }
    }
}
