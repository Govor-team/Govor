using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class InvitationValidator : IObjectValidator<Invitation>
{
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
            if(inv.EndDate < inv.DateCreated)
                throw new ArgumentException("EndDate cannot be less than StartDate", nameof(inv.EndDate));
            if(inv.MaxParticipants <= 0)
                throw new ArgumentException("MaxParticipants cannot be less than 0", nameof(inv.MaxParticipants));
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