using Govor.Domain;

namespace Govor.Application.Authentication.Exceptions;

public class UserNotRegisteredException(string username) : GovorCoreException($"{username} is not registered!") { }