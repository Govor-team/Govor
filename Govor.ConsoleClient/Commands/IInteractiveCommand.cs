namespace Govor.ConsoleClient.Commands;

public interface IInteractiveCommand : ICommand
{
    Task HandleInputAsync(string input);
    bool IsCompleted { get; }
}