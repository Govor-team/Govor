namespace Govor.Data.Repositories.Exceptions;

public class RemoveException(string s, Exception exception)
    : Exception(s, exception);