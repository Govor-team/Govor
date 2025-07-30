namespace Govor.Contracts.DTOs;

public class SignedPreKeyDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty; 
    public string Signature { get; set; } = string.Empty;
}