using Govor.Core.Models.Users.Crypto;

namespace Govor.Core.Models.Users;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty; // "Chrome on Windows" 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    // public DateTime? RevokedAt { get; set; } TODO: Clear old UserSessions 

    public UserCryptoSession CryptoSession { get; set; }

    public override bool Equals(object? obj)
    {
        UserSession? userSession = obj as UserSession;
        
        return Id == userSession.Id &&
               UserId == userSession.UserId &&
               RefreshTokenHash == userSession.RefreshTokenHash &&
               DeviceInfo == userSession.DeviceInfo &&
               CreatedAt == userSession.CreatedAt &&
               ExpiresAt == userSession.ExpiresAt &&
               IsRevoked == userSession.IsRevoked;
    }
}