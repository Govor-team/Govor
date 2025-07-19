namespace Govor.ConsoleClient.Services.Middleware;

public interface ICommandMiddleware
{
    Task InvokeAsync(CommandContext context, Func<Task> next);
}