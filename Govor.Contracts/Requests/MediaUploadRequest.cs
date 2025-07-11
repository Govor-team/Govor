using Govor.Core.Models.Messages;

namespace Govor.Contracts.Requests;

public class MediaUploadRequest
{
    public byte[] Data { get; set; }
    public string FileName { get; set; }
    public string EncryptedKey { get; set; } = string.Empty;
    public MediaType Type { get; set; }
    public string MimeType { get; set; } = string.Empty;
}