using System.ComponentModel.DataAnnotations;

namespace Govor.Core.Models;

public class Admin
{
    [Key]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
