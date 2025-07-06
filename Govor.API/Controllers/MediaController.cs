using Govor.Application.Interfaces;
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
    
    public MediaController(ILogger<MediaController> logger, IMediaService mediaService)
    {
        _logger = logger;
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)]// ~100MB 
    public async Task<IActionResult> Upload([FromForm] MediaUploadRequest request)
    {
        try
        {
            var result = await _mediaService.UploadMediaAsync(new Media(request.Data, request.FileName, request.Type,
                request.MimeType, request.EncryptedKey));
            
           return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }
}