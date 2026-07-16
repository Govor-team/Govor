using Govor.Domain;

namespace Govor.Application.Exceptions.FriendsService;

public class RequestAlreadySentException(Guid fromId, Guid userId) 
    : GovorCoreException($"Request was already sent from {fromId} to {userId}") {}
