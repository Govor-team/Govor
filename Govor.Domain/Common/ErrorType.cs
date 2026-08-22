namespace Govor.Domain.Common;

public enum ErrorType
{
    Failure = 0,      // (400 Bad Request)
    Validation = 1,   // (400 Bad Request / 422 Unprocessable)
    NotFound = 2,     // (404 Not Found)
    Conflict = 3,     // (409 Conflict)
    Unauthorized = 4, // (401 Unauthorized)
    Forbidden = 5,    // (403 Forbidden)
    ServerError = 6   // (500 Server Error)
}