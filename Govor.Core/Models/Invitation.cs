using Govor.Core.Models.Users;

namespace Govor.Core.Models;

public class Invitation
{
    public Guid Id { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public string Code { get; set; }
    public string Description { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxParticipants { get; set; }
    public List<User> Users { get; set; } = new List<User>();

    public override bool Equals(object? obj)
    {
        var invitation = obj as Invitation ?? throw new InvalidCastException();

        return Id == invitation.Id &&
               IsAdmin == invitation.IsAdmin &&
               Description == invitation.Description &&
               DateCreated == invitation.DateCreated &&
               EndDate == invitation.EndDate &&
               MaxParticipants == invitation.MaxParticipants;
    }
}