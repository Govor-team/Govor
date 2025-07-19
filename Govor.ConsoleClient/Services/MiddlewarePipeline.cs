using Govor.ConsoleClient.Services.Middleware;

namespace Govor.ConsoleClient.Services;

public delegate Task CommandMiddleware(CommandContext context, Func<Task> next);

public class MiddlewarePipeline
{
    private readonly IList<ICommandMiddleware> _middlewares;

    public MiddlewarePipeline(IEnumerable<ICommandMiddleware> middlewares)
    {
        _middlewares = middlewares.ToList();
    }

    public Task ExecuteAsync(CommandContext context)
    {
        return InvokeNext(0, context);
    }

    private Task InvokeNext(int index, CommandContext context)
    {
        if (index < _middlewares.Count)
        {
            return _middlewares[index].InvokeAsync(context, () => InvokeNext(index + 1, context));
        }
        return context.Command.ExecuteAsync(context);
    }
}