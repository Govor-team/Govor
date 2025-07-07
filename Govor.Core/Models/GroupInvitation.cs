namespace Govor.Core.Models;

public class GroupInvitation
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserMakerId { get; set; }
    public string InvitationCode { get; set; }
    public string Description {get; set;}
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MaxParticipants { get; set; }
    public List<Guid> GroupMemberships { get; set; } = new();
    public User? UserMaker { get; set; }
}