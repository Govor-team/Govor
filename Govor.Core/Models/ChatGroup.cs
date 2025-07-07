namespace Govor.Core.Models;

public class ChatGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid ImageId { get; set; }
    public bool IsChannel { get; set; }
    public bool IsPrivate { get; set; }
    public List<GroupAdmins> Admins { get; set; } = new();
    public List<GroupMembership> Members { get; set; } = new();
    public List<GroupInvitation> InviteCodes { get; set; } = new();
}