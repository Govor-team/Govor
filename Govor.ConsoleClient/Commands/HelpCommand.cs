using System.Reflection;
using Govor.ConsoleClient.Services;
using Govor.ConsoleClient.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Govor.ConsoleClient.Commands;

public class HelpCommand : ICommand
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public HelpCommand(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var commands = dispatcher.GetAllCommands();

        if (context.Arguments is not null)
        {
            var command = commands.FirstOrDefault(c => 
                (c.GetType().GetCustomAttribute<CommandRouteAttribute>()?.Path.Replace("/", "").ToLower()
                 ?? c.GetType().Name.Replace("Command", "").ToLower()) == context.Arguments.ToLower());

            if (command != null)
            {
                _logger.Info($"{context.Arguments} - {command.LongHelp()}");
            }
            else
            {
                _logger.Warn("Unknown command");
            }
        }
        else
        {
            _logger.Info("Чтобы получить подробную информацию, напишите /help {command}");
            foreach (var command in commands)
            {
                var name = command.GetType().GetCustomAttribute<CommandRouteAttribute>()?.Path.Replace("/", "").ToLower()
                           ?? command.GetType().Name.Replace("Command", "").ToLower();
            
                _logger.Log($"{name} - {command.ShortHelp()}");
            }
        }
        return Task.CompletedTask;
    }

    public string LongHelp() => "Необходима для получения информации о доступных командах\nЧтобы получить подробную информацию о команде, напишите /help {command}";

    public string ShortHelp() => "Необходима для получения информации о доступных командах";
}
