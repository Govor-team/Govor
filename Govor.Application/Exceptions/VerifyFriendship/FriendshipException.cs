using Govor.Core;

namespace Govor.Application.Exceptions.VerifyFriendship;

public class FriendshipException : GovorCoreException
{
    public FriendshipException(string s)
        :base(s) { }
}