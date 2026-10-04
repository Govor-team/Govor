using Govor.Domain.Models;

namespace Govor.Contracts.Responses.SignalR;

public class GroupProfileChangedResponse
{
    public Guid GroupId { get; set; }
}

public enum GroupMemberStatus { Active, Banned, Left }

public class GroupMemberChangedResponse
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public GroupMemberStatus Status { get; set; }
    public GroupRole? Role { get; set; }
}
