using Govor.Core;

namespace Govor.Data.Repositories.Exceptions;

public class NotFoundException : GovorCoreException
{
    public NotFoundException(string message)
        : base(message) {}
    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException){}
}