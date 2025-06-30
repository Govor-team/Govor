using System.ComponentModel.DataAnnotations;
using Govor.Core.Infrastructure.Validators;

namespace Govor.Contracts.Requests;

public record RegistrationRequest
{
    [Required]
    [StringLength(UserValidator.MAX_LENGHT_OF_NAME,
        MinimumLength = UserValidator.MIN_LENGHT_OF_NAME, 
        ErrorMessage = "Username must be between 4 and 44 characters.")]
    public string Name { get; init; }
    [Required]
    [MinLength(8)]
    public string Password { get; init; }
    [MinLength(InvitationValidator.MIN_INVITATION_LENGTH)]
    public string InviteLink { get; init; }
}
