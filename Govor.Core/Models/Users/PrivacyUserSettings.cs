namespace Govor.Core.Models.Users;

public class PrivacyUserSettings
{
    public Guid UserId { get; set; }
    public bool IsGlobalAccount  { get; set; }   
    
    public WhoCan CanSend { get; set; }
    public List<Guid>? WhitelistSent { get; set; }
    public List<Guid>? BlacklistSent { get; set; }
    
    public WhoCan CanSeeTimeWas  { get; set; }
    public List<Guid>? WhitelistTimeWas { get; set; }
    public List<Guid>? BlacklistTimeWas { get; set; }
    
    public WhoCan CanSeeImage  { get; set; }
    public List<Guid>? WhitelistSeeImage { get; set; }
    public List<Guid>? BlacklistSeeImage{ get; set; }
    
    public DeletingMessagesVia Via { get; set; } // if min value = none 
    public int DeletingIn { get; set; }
}

public enum WhoCan
{
    None = 0,
    OnlyFriends = 1,
    EveryoneCanSend = 2,
}

public enum DeletingMessagesVia
{
    None = 0,
    Hours = 1,
    Days = 2,
    Months = 3,
    Years = 4
}

