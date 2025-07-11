using Govor.Core.Models.Users;

namespace Govor.Core.Models;

public class Friendship
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public FriendshipStatus Status { get; set; }

    public User Requester { get; set; }
    public User Addressee { get; set; }
}

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Rejected,
    Blocked
}
