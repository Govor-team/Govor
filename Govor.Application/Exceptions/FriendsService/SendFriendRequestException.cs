using Govor.Core;

namespace Govor.Application.Exceptions.FriendsService;

public class SendFriendRequestException(Guid fromUserId, Guid toUserId, Exception exception)
    : GovorCoreException($"Something happened when we try to send request from {fromUserId} to {toUserId}", exception);