namespace Govor.Core.Models;

public class GroupMembership
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; } 
    public Guid UserId { get; set; }
    public Guid? InvitationId { get; set; }
    public bool IsBanned { get; set; }
}