using Govor.Domain.Models;

namespace Govor.Contracts.Responses;

public class GroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ImageId { get; set; }
    public string? ImageUrl => ImageId == Guid.Empty ? null : $"/api/media/download/{ImageId}";
    public bool IsPrivate { get; set; }
    public bool IsChannel { get; set; }
    public Guid? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public GroupRole? MyRole { get; set; }
    public bool IsRequiredChannel { get; set; }
    public bool CanLeave { get; set; }
}

public class GroupMemberResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public GroupRole Role { get; set; }
    public bool IsBanned { get; set; }
    public DateTime MemberSince { get; set; }
}

public class GroupInvitationResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string InvitationCode { get; set; } = string.Empty;
    public string SharePath => $"/invite/{InvitationCode}";
    public string JoinPath => $"/api/group-invites/{InvitationCode}/join";
    public string PreviewPath => $"/api/group-invites/{InvitationCode}";
    public string Description { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public int MaxParticipants { get; set; }
    public int UsedCount { get; set; }
    public bool IsRevoked { get; set; }
}
