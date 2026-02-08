namespace Govor.Contracts.Requests;

public class UploadKeysRequest
{
    public byte[] IdentityKey { get; set; }
    public byte[] SignedPreKey { get; set; }
    public byte[] SignedPreKeySignature { get; set; }
    public List<byte[]> OneTimePreKeys { get; set; } = new();
}