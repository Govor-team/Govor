using Govor.Core;

namespace Govor.Data.Repositories.Exceptions;

public class UpdateException : GovorCoreException
{
    public UpdateException(string s)
        : base(s) { }

    public UpdateException(string s, Exception ex)
        : base(s, ex) { }
}