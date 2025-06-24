using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class AdminValidator : IObjectValidator<Admin>
{
    public void Validate(Admin admin)
    {
        try
        {
            if(admin is null)
                throw new ArgumentNullException(nameof(admin));
            if(admin.UserId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(admin.UserId));
        }
        catch (Exception e)
        {
            throw new InvalidObjectException<Admin>(e);
        }
    }

    public bool TryValidate(Admin admin)
    {
        try
        {
            Validate(admin);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}