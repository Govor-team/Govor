using Govor.Application.Interfaces;
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
    private readonly IStorageService _storageService;

    public MediaController(ILogger<MediaController> logger, IStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)]// ~100MB 
    public async Task<IActionResult> Upload([FromForm] MediaUploadRequest request)
    {
        try
        {
           var url = await _storageService.SaveAsync(request.Data,request.FileName);
           var mediaId = Guid.NewGuid();
           
           
           return Ok(mediaId);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
    }
}