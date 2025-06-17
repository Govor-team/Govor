using Govor.Core.Models;
using ArgumentNullException = System.ArgumentNullException;

namespace Govor.Core.Infrastructure.Validators;

public class UserValidator : IObjectValidator<User>
{
    public void Validate(User user)
    {
        try
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if(user.Id == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(user.Id));
            if(user.HashPassword is null || user.HashPassword == string.Empty)
                throw new ArgumentException("Password cannot be empty", nameof(user.HashPassword));
            if(user.CreatedOn == DateTime.MinValue)
                throw new ArgumentException("Time of creation account cannot be empty", nameof(user.CreatedOn));
        }
        catch(Exception ex) 
        {
            throw new InvalidObjectException<User>(ex);
        }
    }

    public bool TryValidate(User objectToValidate)
    {
        try
        {
            Validate(objectToValidate);
            return true;
        }
        catch (InvalidObjectException<User> ex)
        {
            return false;
        }
    }
}