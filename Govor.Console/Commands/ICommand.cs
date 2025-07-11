namespace Govor.ConsoleClient.Commands
{
    public interface ICommand
    {
        Task ExecuteAsync(string? argument);
        string GetHelp();
    }
}
