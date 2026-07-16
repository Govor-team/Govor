using Govor.Domain.Models;

namespace Govor.Contracts.DTOs;

public class FriendshipDto
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public FriendshipStatus Status { get; set; }
}