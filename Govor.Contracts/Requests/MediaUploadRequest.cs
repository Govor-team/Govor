using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Govor.Contracts.Requests;

public class MediaUploadRequest
{
    [Required]
    public IFormFile FromFile { get; set; }
    [Required]
    public MediaType Type { get; set; }
    [Required, MaxLength(255)]
    public string MimeType { get; set; } = string.Empty;
    [Required]
    public string EncryptedKey { get; set; } = string.Empty;
    [Required]
    public MediaOwnerType OwnerType { get; set; } = MediaOwnerType.Message;
}