namespace Govor.Application.Messages;

public class MessageEditingOptions
{
    public bool Enabled { get; set; }
    public int MaxEditTimeMinutes { get; set; } = 15;
}