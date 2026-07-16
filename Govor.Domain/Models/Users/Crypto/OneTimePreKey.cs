namespace Govor.Domain.Models.Users.Crypto;

public class OneTimePreKey
{
    public Guid Id { get; set; }
    public Guid UserCryptoSessionId { get; set; }
    public UserCryptoSession UserCryptoSession { get; set; }

    public byte[] PublicKey { get; set; }
    public bool IsUsed { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
