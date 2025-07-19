namespace Govor.ConsoleClient.Services;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    public void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ResetColor();
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }
    
    public void Title(string message)
    {
        var upper = message.ToUpper();
        var length = upper.Length + 6;
        var border = new string('=', length);
        var padded = $"=  {upper}  =";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(border);
        Console.WriteLine(padded);
        Console.WriteLine(border);
        Console.ResetColor();
    }
}