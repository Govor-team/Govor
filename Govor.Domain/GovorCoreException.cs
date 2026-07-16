namespace Govor.Domain;

/// <summary>
/// Base exception class for Govor solutions 
/// </summary>
public class GovorCoreException : Exception
{
    public GovorCoreException() { }
    
    public GovorCoreException(string message) 
        : base(message) { }
    
    public GovorCoreException(string message, Exception innerException)
        : base(message, innerException) { }
}