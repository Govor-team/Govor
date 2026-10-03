using System.ComponentModel.DataAnnotations;

namespace Govor.Contracts.Requests;

public class SetPasswordRequest
{
    [Required, MinLength(8), MaxLength(256)]
    public string Password { get; set; } = string.Empty;
}
