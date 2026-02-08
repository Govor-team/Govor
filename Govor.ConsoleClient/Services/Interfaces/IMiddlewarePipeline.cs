namespace Govor.ConsoleClient.Services.Interfaces;

public interface IMiddlewarePipeline
{
    Task ExecuteAsync(CommandContext context);
}