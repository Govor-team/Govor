namespace Govor.Data.Repositories.Exceptions;

public class UpdateException(string s, Exception ex)
    : Exception(s, ex);