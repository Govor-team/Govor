namespace Govor.API.Hubs.Infrastructure;

public static class ChatHubConstants
{
    public const string ReceiveMessage = "ReceiveMessage";
    public const string MessageSent = "MessageSent";
    public const string MessageRemoved = "MessageRemoved";
    public const string MessageEdited = "MessageEdited";
    public static string MessageRead = "MessageReaded";
    public const string ChatRead = "ChatRead";
    public const string MessageReactionsChanged = "MessageReactionsChanged";
    public const string ChannelReactionPolicyChanged = "ChannelReactionPolicyChanged";

    public static string GetUserGroup(Guid userId) => userId.ToString();
    public static string GetSessionGroup(Guid sessionId) => $"session_{sessionId}";
    public static string GetChatGroup(Guid groupId) => $"group_{groupId}";
    public static string GetPrivateChat(Guid groupId) => $"private_{groupId}";
}
