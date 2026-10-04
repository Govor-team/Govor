using System.ComponentModel.DataAnnotations;

namespace Govor.Contracts.Requests;

public class CreateGroupRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; } = true;
    public bool IsChannel { get; set; }
}

public class UpdateGroupRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
}

public class CreateGroupInvitationRequest
{
    [Range(1, 365)] public int ValidForDays { get; set; } = 7;
    [Range(0, 1000000)] public int MaxParticipants { get; set; }
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
}

public class RequiredChannelRequest
{
    public Guid? ChannelId { get; set; }
    public bool AllowLeave { get; set; } = true;
}
