namespace Govor.Application.Messages.Parameters;

public record SendMedia(Guid MediaId, string EncryptedKey);