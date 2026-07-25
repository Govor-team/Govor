using Govor.Domain.Common;
using SmartRes;

namespace Govor.Application.Infrastructure.Validators;

public interface IUsernameValidator
{
    Result<Unit, Error> Validate(string username);
    bool TryValidate(string username);
}