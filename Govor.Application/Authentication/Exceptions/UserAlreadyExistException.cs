using Govor.Domain;

namespace Govor.Application.Authentication.Exceptions;

public class UserAlreadyExistException(string username) : GovorCoreException($"{username} is already exists!") { }