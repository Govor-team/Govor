using Govor.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using SmartRes;

namespace Govor.API.Common.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T, Error> result)
    {
        if (result.IsSuccess)
        {
            // Если тип Unit, возвращаем 204 No Content, иначе 200 OK со значением
            return typeof(T) == typeof(Unit) 
                ? new StatusCodeResult(StatusCodes.Status204NoContent) 
                : new OkObjectResult(result.Value);
        }

        return GenerateProblemDetails(result.Error);
    }
    
    private static ActionResult GenerateProblemDetails(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitleForErrorType(error.Type),
            Detail = error.Message,
        };

        problemDetails.Extensions.Add("errorCode", error.Code);
        
        if (error.Errors is not null)
        {
            problemDetails.Extensions.Add("errors", error.Errors);
        }

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private static string GetTitleForErrorType(ErrorType type) => type switch
    {
        ErrorType.NotFound => "Not Found",
        ErrorType.Validation => "Validation Error",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        _ => "Bad Request"
    };
}