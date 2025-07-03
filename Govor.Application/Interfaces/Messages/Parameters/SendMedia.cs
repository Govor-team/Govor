using Govor.Core.Models;

namespace Govor.Application.Interfaces.Messages.Parameters;

public record SendMedia(Guid Id,  
    string EncryptedKey, 
    MediaType Type,
    string MimeType);