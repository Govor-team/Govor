namespace Govor.Data.Repositories.Exceptions;

public class AdditionException(string s, Exception ex) 
    : Exception(s, ex);