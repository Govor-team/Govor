using Govor.Core;

namespace Govor.Application.Exceptions.FriendsService;

public class SearchUsersException : GovorCoreException
{
    public SearchUsersException(string message, Exception innerException)
        : base(message, innerException){}

    public SearchUsersException(string message) 
        : base(message){}
   
}