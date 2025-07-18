namespace Govor.Core.Models;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty; // например, "Chrome on Windows"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    public override bool Equals(object? obj)
    {
        UserSession? userSession = obj as UserSession;
        
        return Id == userSession.Id &&
               UserId == userSession.UserId &&
               RefreshToken == userSession.RefreshToken &&
               DeviceInfo == userSession.DeviceInfo &&
               CreatedAt == userSession.CreatedAt &&
               ExpiresAt == userSession.ExpiresAt &&
               IsRevoked == userSession.IsRevoked;
    }
}