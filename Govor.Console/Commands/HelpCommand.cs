using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class HelpCommand : BaseCommand
    {
        private readonly Dictionary<string, ICommand> _availableCommands;

        // The HelpCommand needs access to the list of all commands to display their help messages.
        // This will be injected from Program.cs where the commands are registered.
        public HelpCommand(Dictionary<string, ICommand> availableCommands)
        {
            _availableCommands = availableCommands;
        }

        public override Task ExecuteAsync(string? argument)
        {
            Console.WriteLine("Доступные команды:");
            if (!string.IsNullOrWhiteSpace(argument))
            {
                string commandKey = argument.StartsWith("/") ? argument : "/" + argument;
                if (_availableCommands.TryGetValue(commandKey.ToLower(), out var specificCommand))
                {
                    Console.WriteLine(specificCommand.GetHelp());
                }
                else
                {
                    Console.WriteLine($"Команда '{argument}' не найдена. Введите /help для списка всех команд.");
                }
            }
            else
            {
                foreach (var commandEntry in _availableCommands.OrderBy(c => c.Key))
                {
                    Console.WriteLine(commandEntry.Value.GetHelp());
                }
            }
            return Task.CompletedTask;
        }

        public override string GetHelp()
        {
            return "/help [команда] - Показать список всех команд или помощь по конкретной команде.";
        }
    }
}
