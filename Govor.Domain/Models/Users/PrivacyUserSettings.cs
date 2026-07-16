namespace Govor.Domain.Models.Users;

public class PrivacyUserSettings
{
    public Guid UserId { get; set; }

    public bool IsGlobalAccount { get; set; }

    public DeletingMessagesVia DeletingVia { get; set; }
    public int DeletingIn { get; set; }

    public bool IsInvisibleMode { get; set; }

    public List<PrivacyRuleEntity> Rules { get; set; } = new();
}

public enum WhoCan
{
    None = 0,
    OnlyFriends = 1,
    Everyone = 2,
}

public enum DeletingMessagesVia
{
    None = 0,
    Hours = 1,
    Days = 2,
    Months = 3,
    Years = 4
}

public enum PrivacyTargetArea
{
    CanSend = 0,
    CanSeeTimeWas = 1,
    CanSeeImage = 2,
    CanSendImage = 3,
}

public enum PrivacyRuleType
{
    Allow = 0,
    Deny = 1
}


