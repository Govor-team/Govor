using SmartRes;

namespace Govor.Domain.Common;

public static class Result
{
    // Для методов, возвращающих значение
    public static Result<T, Error> Success<T>(T value) => Result<T, Error>.Success(value);
    public static Result<T, Error> Failure<T>(Error error) => Result<T, Error>.Failure(error);

    // Для методов, которые раньше возвращали просто Result (без T)
    public static Result<Unit, Error> Success() => Result<Unit, Error>.Success(Unit.Value);
    public static Result<Unit, Error> Failure(Error error) => Result<Unit, Error>.Failure(error);
    
    public static Result<Unit, Error> Failure(Exception ex) => 
        Result<Unit, Error>.Failure(new Error(ex.GetType().Name, ex.Message, ErrorType.Failure));
        
    public static Result<T, Error> Failure<T>(Exception ex) => 
        Result<T, Error>.Failure(new Error(ex.GetType().Name, ex.Message, ErrorType.Failure));
}