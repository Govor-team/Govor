namespace Govor.Application.Synching;

public interface ISynchingService
{
    /// <summary>
    /// Brings all line breaks (CRLF, CR) to a single LF(\n) standard.
    /// </summary>
    /// <param name="input">The original line containing the line break.</param>
    /// <returns>A string normalized using only \n.</returns>
    string NormalizeNewlines(string input);
}