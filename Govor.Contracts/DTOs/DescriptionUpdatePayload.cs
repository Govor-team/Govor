namespace Govor.Contracts.DTOs;

public class DescriptionUpdatePayload
{
    public Guid UserId { get; set; }
    public string? Description { get; set; }
}