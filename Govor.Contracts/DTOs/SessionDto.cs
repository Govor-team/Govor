namespace Govor.Contracts.DTOs;

public record SessionDto
{
    public Guid Id { get; init; }
    public string DeviceInfo { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
}
