using Govor.Core.Models;

namespace Govor.Application.Interfaces.Messages.Parameters;

public record SendMedia(Guid MediaId, string EncryptedKey);