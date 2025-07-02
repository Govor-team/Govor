using Govor.Core.Models;

namespace Govor.Contracts.Requests.SignalR;

public record MediaReference
{
    public Guid MediaId { get; init; }
    public string EncryptedKey { get; init; } = string.Empty;
    public MediaType Type { get; init; }
    public string MimeType { get; init; } = string.Empty;
}