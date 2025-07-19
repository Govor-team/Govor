using Govor.ConsoleClient.Services;
using Govor.ConsoleClient.Services.Interfaces;

namespace Govor.ConsoleClient;

public class App
{
    private readonly IInputPipeline _inputPipeline;
    private readonly ILogger _logger;
    
    public App(IInputPipeline inputPipeline, ILogger logger)
    {
        _logger = logger;
        _inputPipeline = inputPipeline;
    }

    public async Task RunAsync()
    {
        _logger.Title("Добро пожаловать в консольный клиент Говор!");
        while (true)
        {
            Console.Write(">> ");
            var input = Console.ReadLine();
            if (input == null || input.Trim().ToLower() == "exit") break;
            await _inputPipeline.ProcessInputAsync(input);
        }
    }
}
