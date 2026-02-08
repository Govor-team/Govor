using Govor.ConsoleClient.Commands;
using Govor.ConsoleClient.Services.Interfaces;

namespace Govor.ConsoleClient.Services.Implementations;

public class InputPipeline : IInputPipeline
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly ILogger _logger;
    private IInteractiveCommand? _activeCommand;
    
    public InputPipeline(ICommandDispatcher dispatcher, ILogger logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task ProcessInputAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        if (input.StartsWith("/"))
        {
            _activeCommand = null; 

            var commandInput = input[1..];
            var result = await _dispatcher.DispatchAsync(commandInput);

            // If the command supports interactivity, save it as active
            if (result is IInteractiveCommand interactiveCommand && !interactiveCommand.IsCompleted)
            {
                _activeCommand = interactiveCommand;
            }
        }
        else
        {
            if (_activeCommand != null)
            {
                await _activeCommand.HandleInputAsync(input);

                if (_activeCommand.IsCompleted)
                    _activeCommand = null;
            }
            else
            {
                _logger.Info($"Введите /help, чтобы узнать доступные команды!");
            }
        }
    }
}