namespace Govor.Contracts.DTOs;

public class PublicSessionKeysDto
{
    public string IdentityKey { get; set; } = string.Empty; // base64
    public SignedPreKeyDto SignedPreKey { get; set; }
    public List<OneTimePreKeyDto> OneTimePreKeys { get; set; } = new();
}
