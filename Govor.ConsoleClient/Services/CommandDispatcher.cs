using System.Reflection;
using Govor.ConsoleClient.Commands;

namespace Govor.ConsoleClient.Services;

public class CommandDispatcher
{
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly ILogger _logger;
    private readonly MiddlewarePipeline _pipeline;

    public CommandDispatcher(IEnumerable<ICommand> commands, ILogger logger, MiddlewarePipeline pipeline)
    {
        _logger = logger;
        _pipeline = pipeline;

        foreach (var command in commands)
        {
            var route = command.GetType().GetCustomAttribute<CommandRouteAttribute>()?.Path.Replace("/","")
                        ?? command.GetType().Name.Replace("Command", "").ToLower();
            _commands[route.ToLower()] = command;
        }
    }
    
    public async Task<ICommand?> DispatchAsync(string input)
    {
        var args = input.Split(' ', 2);
        var cmd = args[0].ToLower();
        if (_commands.TryGetValue(cmd, out var command))
        {
            var context = new CommandContext(cmd, args.Length > 1 ? args[1] : null, command);
            await _pipeline.ExecuteAsync(context);
            return command;
        }
        else
        {
            _logger.Warn("Неизвестная команда. Введите '/help'.");
            return null;
        }
    }
}