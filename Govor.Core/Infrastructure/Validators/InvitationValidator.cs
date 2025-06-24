using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class InvitationValidator : IObjectValidator<Invitation>
{
    public const int MIN_INVITATION_LENGTH = 10;
    public void Validate(Invitation inv)
    {
        try
        {
            if(inv is null)
                throw new ArgumentNullException(nameof(inv));
            if(inv.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(inv.Id));
            if(inv.DateCreated == DateTime.MinValue)
                throw new ArgumentException("DateCreated cannot be empty", nameof(inv.DateCreated));
            if(inv.EndDate < inv.DateCreated && inv.IsActive)
                throw new ArgumentException("EndDate cannot be less than StartDate when is active", nameof(inv.EndDate));
            if((inv.MaxParticipants <= 0 || inv.MaxParticipants <= inv.Users.Count) && inv.IsActive)
                throw new ArgumentException("MaxParticipants cannot be less than 0 or users cannot be more then MaxParticipants when is active", nameof(inv.MaxParticipants));
            if(inv.Code == string.Empty || inv.Code.Length < MIN_INVITATION_LENGTH)
                throw new ArgumentException($"Code cannot be empty or less then {MIN_INVITATION_LENGTH}", nameof(inv.Code));
        }
        catch (Exception ex)
        {
            throw new InvalidObjectException<Invitation>(ex);
        }
    }

    public bool TryValidate(Invitation inv)
    {
        try
        {
            Validate(inv);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}