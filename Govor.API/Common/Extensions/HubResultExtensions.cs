using Govor.Contracts.Responses.SignalR;
using Govor.Domain.Common;

namespace Govor.API.Common.Extensions;

public static class HubResultExtensions
{
    public static HubResult<T> ToHubResult<T>(this Error error) => error.Type switch
    {
        ErrorType.NotFound => HubResult<T>.NotFound(error.Message),
        ErrorType.Forbidden => HubResult<T>.Forbidden(error.Message),
        ErrorType.Unauthorized => HubResult<T>.Unauthorized(error.Message),
        ErrorType.Conflict => HubResult<T>.Conflict(error.Message),
        ErrorType.ServerError => HubResult<T>.Error("Internal server error."),
        _ => HubResult<T>.BadRequest(error.Message)
    };
}
