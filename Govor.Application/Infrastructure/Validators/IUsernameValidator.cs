using Govor.Domain.Common;

namespace Govor.Application.Infrastructure.Validators;

public interface IUsernameValidator
{
    Result Validate(string username);
    bool TryValidate(string username);
}