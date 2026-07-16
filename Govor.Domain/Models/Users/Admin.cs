using System.ComponentModel.DataAnnotations;

namespace Govor.Domain.Models.Users;

public class Admin
{
    [Key]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
