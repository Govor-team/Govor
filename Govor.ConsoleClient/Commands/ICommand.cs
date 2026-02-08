using Govor.ConsoleClient.Services;

namespace Govor.ConsoleClient.Commands;

public interface ICommand
{
    Task ExecuteAsync(CommandContext context);
    string LongHelp();
    string ShortHelp();
}

[AttributeUsage(AttributeTargets.Class)]
public class CommandRouteAttribute : Attribute
{
    public string Path { get; }
    public CommandRouteAttribute(string path) => Path = path;
}