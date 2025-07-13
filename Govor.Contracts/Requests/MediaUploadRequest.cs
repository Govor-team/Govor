using System.ComponentModel.DataAnnotations;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.Http;

namespace Govor.Contracts.Requests;

public class MediaUploadRequest
{
    [Required]
    public IFormFile Data { get; set; }
    public string FileName { get; set; }
    public string EncryptedKey { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public string MimeType { get; set; } = string.Empty;
}