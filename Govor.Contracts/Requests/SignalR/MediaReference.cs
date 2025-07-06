using Govor.Core.Models;

namespace Govor.Contracts.Requests.SignalR;

public record MediaReference
{
    public Guid MediaId { get; init; }
    public string EncryptedKey { get; init; } = string.Empty;
}