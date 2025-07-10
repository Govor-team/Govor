using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Govor.Application.Exceptions.AuthService;
using Govor.Application.Interfaces.Authentication;
using Govor.Core.Infrastructure.Validators;

namespace Govor.Application.Infrastructure.Validators;

public class UsernameValidator : IUsernameValidator
{
    private readonly Regex _usernameRegex = new(@"^[А-Яа-яЁё]+[А-Яа-яЁё0-9]*$", RegexOptions.Compiled);
    
    public void Validate(string username)
    {
        if(username.Length < UserValidator.MIN_LENGHT_OF_NAME || username.Length > UserValidator.MAX_LENGHT_OF_NAME)
            throw new InvalidUsernameException($"Username must be between {UserValidator.MIN_LENGHT_OF_NAME} and {UserValidator.MAX_LENGHT_OF_NAME} characters.");

        if (!_usernameRegex.IsMatch(username))
            throw new InvalidUsernameException("The username must be in Cyrillic and start with a letter.");
    }

    public bool TryValidate(string username)
    {
        try
        {
            Validate(username);
            return true;
        }
        catch
        {
            return false;
        }
    }

}