namespace Govor.Core.Models.Users;

public class PrivacyRuleEntity
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    
    public PrivacyTargetArea Area { get; set; }
    public WhoCan AccessType { get; set; } // Everyone, Friends, None

    public List<Guid> Whitelist { get; set; } = new();
    public List<Guid> Blacklist { get; set; } = new();

    public PrivacyUserSettings OwnerSettings { get; set; } = null!;
}