namespace Govor.Application.Interfaces.PushNotifications.Models;

public class PushMessage
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new();
    public string? ChannelId { get; set; } = "chat_messages"; //for Android
}