namespace Govor.Contracts.DTOs;

public class InvitationDto
{
    public Guid Id { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public string Code {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime EndAt { get; set; }
    public int MaxParticipants { get; set; }
    public string Description { get; set; }
}