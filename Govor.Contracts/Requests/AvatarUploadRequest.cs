using System.ComponentModel.DataAnnotations;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.Http;

namespace Govor.Contracts.Requests;

public class AvatarUploadRequest
{
    [Required]
    public IFormFile FromFile { get; set; }
    [Required]
    public MediaType Type { get; set; }
    [Required, MaxLength(255)]
    public string MimeType { get; set; } = string.Empty;
}