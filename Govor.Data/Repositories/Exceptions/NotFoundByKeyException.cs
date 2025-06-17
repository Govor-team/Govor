namespace Govor.Data.Repositories.Exceptions;

public class NotFoundByKeyException<T> : NotFoundException
{
    public NotFoundByKeyException(T key) 
        : base($"Not found object by {key}"){}

    public NotFoundByKeyException(T id, string message) 
        : base($"Not found object by {id}. Message: " + message) {}
    
    public NotFoundByKeyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}