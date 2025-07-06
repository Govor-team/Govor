namespace Govor.Core.Models;

public class ChatGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid ImageId { get; set; }
    
    public List<string> InviteCode { get; set; }
    public bool IsChannel { get; set; }
    public bool IsPrivate { get; set; }
    public List<Guid> Admins { get; set; } = new();
}