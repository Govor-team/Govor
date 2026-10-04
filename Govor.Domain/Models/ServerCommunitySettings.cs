namespace Govor.Domain.Models;

public enum GroupRole { Member, Admin, Owner }

public class ServerCommunitySettings
{
    public int Id { get; set; } = 1;
    public Guid? RequiredChannelId { get; set; }
    public bool AllowLeave { get; set; } = true;
}
