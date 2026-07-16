namespace Govor.Domain.Models.Users.Crypto;

public class SignedPreKey
{
    public Guid Id { get; set; }
    public Guid UserCryptoSessionId { get; set; }
    public UserCryptoSession UserCryptoSession { get; set; }
    public byte[] PublicSignedPreKey { get; set; }
    public byte[] SignedPreKeySignature { get; set; }
}