namespace Govor.Core.Models.Users.Crypto;

public class UserCryptoSession
{
    public Guid Id { get; set; }

    public Guid UserSessionId { get; set; }
    public UserSession UserSession { get; set; }

    public byte[] PublicIdentityKey { get; set; }

    public SignedPreKey SignedPreKey { get; set; }
    public ICollection<OneTimePreKey> OneTimePreKeys { get; set; }
}
