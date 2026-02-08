using Govor.ConsoleClient.Commands;

namespace Govor.ConsoleClient.Services;

public class CommandContext
{
    public string Route { get; }
    public string? Arguments { get; }
    public ICommand Command { get; }

    public CommandContext(string route, string? arguments, ICommand command)
    {
        Route = route;
        Arguments = arguments;
        Command = command;
    }
}