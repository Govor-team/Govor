namespace Govor.Application.Synching;

public class SynchingService : ISynchingService
{
    public string NormalizeNewlines(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        
        string normalized = input.Replace("\r\n", "\n");
        
        normalized = normalized.Replace('\r', '\n');

        return normalized;
    }
}