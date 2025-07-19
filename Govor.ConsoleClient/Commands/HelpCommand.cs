using Govor.ConsoleClient.Services;

namespace Govor.ConsoleClient.Commands;

public class HelpCommand : ICommand
{
    public Task ExecuteAsync(CommandContext context)
    {
        Console.WriteLine("Commands:");
        return Task.CompletedTask;
    }

    public string LongHelp()
    {
        throw new NotImplementedException();
    }

    public string ShortHelp()
    {
        throw new NotImplementedException();
    }
}