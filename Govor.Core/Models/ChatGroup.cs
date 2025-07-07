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

    public override bool Equals(object? obj)
    {
        ChatGroup chatGroup = obj as ChatGroup;
        
        return Id == chatGroup.Id &&
               Name == chatGroup.Name &&
               Description == chatGroup.Description &&
               ImageId == chatGroup.ImageId &&
               IsChannel == chatGroup.IsChannel &&
               IsPrivate == chatGroup.IsPrivate;
    }
}