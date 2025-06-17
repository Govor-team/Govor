namespace Govor.Core.Infrastructure.Validators;

public interface IObjectValidator<T>
{
    void Validate(T objectToValidate);
    bool TryValidate(T objectToValidate);
}

class InvalidObjectException<T>(Exception ex) : GovorCoreException($"The object {typeof(T).FullName} is invalid.", ex);