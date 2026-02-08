using Govor.ConsoleClient.Services;

namespace Govor.ConsoleClient.Commands;

[CommandRoute("/send")]
public class SendMessageCommand : IInteractiveCommand
{
    private string? _recipient;
    private bool _isCompleted;
    public bool IsCompleted => _isCompleted;

    public Task ExecuteAsync(CommandContext context)
    {
        Console.WriteLine("Кому вы хотите отправить сообщение?");
        return Task.CompletedTask;
    }
    
    public async Task HandleInputAsync(string input)
    {
        if (_recipient == null)
        {
            _recipient = input;
            Console.WriteLine("Введите сообщение:");
        }
        else
        {
            var message = input;
            Console.WriteLine($"(Отправка '{message}' пользователю '{_recipient}')");
            _isCompleted = true;
        }

        await Task.CompletedTask;
    }

    public string LongHelp()
    {
        return "Отпарвка тестовых сообщений существующему юзеру 2";
    }

    public string ShortHelp()
    {
        return "Отпарвка тестовых сообщений существующему юзеру";
    }
}