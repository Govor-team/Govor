namespace Govor.Contracts.Requests;

public class RotateOneTimePreKeysRequest
{
    public ICollection<byte[]> NewOneTimePreKeys { get; set; } = new List<byte[]>();
}
