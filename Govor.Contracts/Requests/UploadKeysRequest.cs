namespace Govor.Contracts.Requests;

public class UploadKeysRequest
{
    public string PublicEncryptionKey { get; set; }
    public string PublicSigningKey { get; set; }
}