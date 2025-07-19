using Govor.ConsoleClient.Commands;

namespace Govor.ConsoleClient.Services;

public class InputPipeline
{
    private readonly CommandDispatcher _dispatcher;
    private readonly ILogger _logger;
    private IInteractiveCommand? _activeCommand;
    
    public InputPipeline(CommandDispatcher dispatcher, ILogger logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task ProcessInputAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        if (input.StartsWith("/"))
        {
            _activeCommand = null; // Сброс активной команды

            var commandInput = input[1..];
            var result = await _dispatcher.DispatchAsync(commandInput);

            // Если команда поддерживает интерактивность, сохраняем как активную
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
                _logger.Info($"[Ввод пользователя]: {input}");
            }
        }
    }
}