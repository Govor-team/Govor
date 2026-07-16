using Govor.Contracts.Responses.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Filters;

public class HubExceptionFilter : IHubFilter
{
    private readonly ILogger<HubExceptionFilter> _logger;

    public HubExceptionFilter(ILogger<HubExceptionFilter> logger)
    {
        _logger = logger;
    }
    
     public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request in {Method}", context.HubMethodName);
            return CreateHubErrorResult(context, HubResultStatus.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized in {Method}", context.HubMethodName);
            return CreateHubErrorResult(context, HubResultStatus.Unauthorized, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found in {Method}", context.HubMethodName);
            return CreateHubErrorResult(context, HubResultStatus.NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in {Method}", context.HubMethodName);
            return CreateHubErrorResult(context, HubResultStatus.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in {Method}", context.HubMethodName);
            return CreateHubErrorResult(context, HubResultStatus.ServerError, "Internal server error");
        }
    }

    private static object CreateHubErrorResult(HubInvocationContext context, HubResultStatus status, string message)
    {
        var returnType = context.HubMethod.ReturnType;

        // Поддержка Task<HubResult<T>>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GenericTypeArguments[0];

            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(HubResult<>))
            {
                var hubResultType = resultType;
                var errorResult = Activator.CreateInstance(hubResultType)!;

                hubResultType.GetProperty(nameof(HubResult<object>.Status))!
                    .SetValue(errorResult, status);
                hubResultType.GetProperty(nameof(HubResult<object>.ErrorMessage))!
                    .SetValue(errorResult, message);

                return Task.FromResult(errorResult);
            }
        }

        return null;
    }
}