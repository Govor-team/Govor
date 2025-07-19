using Govor.ConsoleClient.Commands;

namespace Govor.ConsoleClient.Services.Interfaces;

public interface ICommandDispatcher
{
    Task<ICommand?> DispatchAsync(string input);
    IEnumerable<ICommand> GetAllCommands();
}
