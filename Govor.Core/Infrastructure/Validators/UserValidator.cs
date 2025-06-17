using Govor.Core.Models;
using ArgumentNullException = System.ArgumentNullException;

namespace Govor.Core.Infrastructure.Validators;

public class UserValidator : IObjectValidator<User>
{
    public const int MIN_LENGHT_OF_NAME = 4;
    public const int MAX_LENGHT_OF_NAME = 100;
    public void Validate(User user)
    {
        try
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if(user.Id == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(user.Id));
            if(user.Username is null 
               || user.Username.Length < MIN_LENGHT_OF_NAME 
               || user.Username.Length > MAX_LENGHT_OF_NAME)
                throw new ArgumentException($"Username cannot be empty or less then {MIN_LENGHT_OF_NAME} chars or more then {MAX_LENGHT_OF_NAME}", nameof(user.Username));
            if(user.HashPassword is null || user.HashPassword == string.Empty)
                throw new ArgumentException("Password cannot be empty", nameof(user.HashPassword));
            if(user.CreatedOn == DateOnly.MinValue)
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