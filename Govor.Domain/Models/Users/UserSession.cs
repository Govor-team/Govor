using Govor.Domain.Models.Users.Crypto;

namespace Govor.Domain.Models.Users;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    
    public User User { get; set; } = null!; 
    public UserCryptoSession CryptoSession { get; set; } = null!;

    public override bool Equals(object? obj)
    {
        if (obj is not UserSession userSession) 
            return false;
        
        return Id == userSession.Id &&
               UserId == userSession.UserId &&
               RefreshTokenHash == userSession.RefreshTokenHash &&
               DeviceInfo == userSession.DeviceInfo &&
               CreatedAt == userSession.CreatedAt &&
               ExpiresAt == userSession.ExpiresAt &&
               IsRevoked == userSession.IsRevoked;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, UserId, RefreshTokenHash, DeviceInfo, CreatedAt, ExpiresAt, IsRevoked);
    }
}