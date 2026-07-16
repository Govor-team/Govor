using Govor.Domain;

namespace Govor.Application.Authentication.Exceptions;

public class InvalidUsernameException(string message) : GovorCoreException(message)
{
    
}