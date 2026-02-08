namespace Govor.ConsoleClient.Services.Interfaces;

public interface IInputPipeline
{
    Task ProcessInputAsync(string input);
}