using Govor.Core;

namespace Govor.Application.Exceptions.AuthService;

public class UserNotRegisteredException(string username) : GovorCoreException($"{username} is not registered!") { }