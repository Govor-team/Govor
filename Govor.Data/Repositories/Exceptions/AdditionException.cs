namespace Govor.Data.Repositories.Exceptions;

public class AdditionException : Exception
{
    public AdditionException(string s, Exception ex) : base(s, ex)
    {
    }
    
    public AdditionException(string s) : base(s) { }
}