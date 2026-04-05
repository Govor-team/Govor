using FirebaseAdmin.Messaging;
using Govor.Application.Interfaces.PushNotifications;
using Govor.Application.Interfaces.PushNotifications.Models;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.PushNotifications.Providers;

public class FirebasePushProvider : IPushNotificationProvider
{
    private readonly ILogger<FirebasePushProvider> _logger;
    public FirebasePushProvider(ILogger<FirebasePushProvider> logger)
    {
        _logger = logger;
    }
    
    public string Name => "FCM";

    public async Task<SendPushResult> SendToTokenAsync(string token, PushMessage message)
    {
        var msg = BuildFcmMessage(message, token: token);

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(msg);
            return new SendPushResult(1, 0, []);
        }
        catch (FirebaseMessagingException ex)
        {
            if (IsInvalidTokenError(ex))
            {
                _logger.LogError(ex, "FCM send failed");
                return new SendPushResult(0, 1,  [token]);
            }

            throw; // return failed
        }
    }

    public async Task<SendPushResult> SendMulticastAsync(IReadOnlyList<string> tokens, PushMessage message)
    {
        if (tokens.Count == 0)
            return new SendPushResult(0, 0, []);
        
        var multicastMsg = new MulticastMessage
        {
            Tokens = tokens.ToList(),
            Notification = new Notification
            {
                Title = message.Title,
                Body  = message.Body,
            },
            Data = message.Data,
            Android = message.ChannelId != null ? new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = message.ChannelId ?? "chat_messages",
                    Tag = message.Tag,
                }
            } : null
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMsg);

            var failedTokens = new List<string>();

            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    var ex = response.Responses[i].Exception;

                    if (ex != null && IsInvalidTokenError(ex))
                    {
                        failedTokens.Add(tokens[i]); // invalid
                    }
                }
            }

            return new SendPushResult(
                response.SuccessCount,
                response.FailureCount,
                failedTokens
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM multicast failed");
            return new SendPushResult(0, tokens.Count, tokens.ToList());
        }
    }

    private Message BuildFcmMessage(PushMessage pm, string? token = null)
    {
        var msg = new Message
        {
            Notification = new Notification
                { 
                    Title = pm.Title,
                    Body = pm.Body
                },
            Data = pm.Data,
            Token = token
        };

        if (pm.ChannelId is not null)
        {
            msg.Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = pm.ChannelId,
                    Tag = pm.Tag,
                }
            };
        }

        return msg;
    }

    private static bool IsInvalidTokenError(FirebaseMessagingException ex)
    {
        return ex.MessagingErrorCode is MessagingErrorCode.Unregistered
                                       or MessagingErrorCode.InvalidArgument
                                       or MessagingErrorCode.SenderIdMismatch;
    }
}