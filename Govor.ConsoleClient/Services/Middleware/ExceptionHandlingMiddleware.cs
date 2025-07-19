using Govor.ConsoleClient.Services.Interfaces;

namespace Govor.ConsoleClient.Services.Middleware;

public class ExceptionHandlingMiddleware : ICommandMiddleware
{
    private readonly ILogger _logger;

    public ExceptionHandlingMiddleware(ILogger logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(CommandContext context, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            _logger.Error($"Произошла ошибка при выполнении команды '{context?.Route}': {ex.Message}");
        }
    }
}