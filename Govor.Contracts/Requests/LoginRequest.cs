using System.ComponentModel.DataAnnotations;
using Govor.Domain.Common.Constants;

namespace Govor.Contracts.Requests;

public class LoginRequest
{
    [Required]
    [StringLength(UserConstants.MAX_LENGHT_OF_NAME,
        MinimumLength = UserConstants.MIN_LENGHT_OF_NAME, 
        ErrorMessage = "Username must be between 4 and 44 characters.")]
    public string Name { get; init; }
    [Required]
    [MinLength(8)]
    public string Password { get; init; }
    public string DeviceInfo { get; init; }
}