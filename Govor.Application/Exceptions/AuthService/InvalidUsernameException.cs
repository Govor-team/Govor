using Govor.Core;

namespace Govor.Application.Exceptions.AuthService;

public class InvalidUsernameException(string message) : GovorCoreException(message)
{
    
}