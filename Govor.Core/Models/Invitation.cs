namespace Govor.Core.Models;

public class Invitation
{
    public Guid Id { get; set; }
    public bool IsAdmin { get; set; }
    public string Description { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxParticipants { get; set; }
    public List<User> Users { get; set; } = new List<User>();
}