using Govor.Core;

namespace Govor.Application.Exceptions.AuthService;

public class UserAlreadyExistException(string username) : GovorCoreException($"{username} is already exists!") { }