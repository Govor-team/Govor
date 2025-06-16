namespace Govor.Core.Models;

public class GroupAdmins
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
}