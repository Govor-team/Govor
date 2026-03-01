using FirebaseAdmin.Messaging;
using Govor.Application.Interfaces.PushNotifications;
using Govor.Application.Interfaces.PushNotifications.Models;
using Govor.Core.Repositories.PushTokens;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.PushNotifications;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushNotificationProvider _provider;
    private readonly IPushTokenRepository _tokenRepo; 
    private readonly ILogger _logger;

    public PushNotificationService(
        IPushNotificationProvider provider,
        IPushTokenRepository tokenRepo,
        ILogger<PushNotificationService> logger)
    {
        _provider = provider;
        _tokenRepo = tokenRepo;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, string channelId, Dictionary<string, string>? data = null)
    {
        var tokens = await _tokenRepo.GetActiveTokensAsync(userId);
        if (tokens.Count == 0)
        {
            _logger.LogInformation("No active push tokens for user {UserId}", userId);
            return;
        }

        var message = new PushMessage
        {
            Title = title,
            Body = body,
            Data = data ?? new(),
            ChannelId = channelId
        };

        var result = await _provider.SendMulticastAsync(tokens, message);

        if (result.FailureCount > 0)
        {
            await _tokenRepo.RemoveTokensAsync(result.FailedTokens);
            _logger.LogWarning("Removed {Count} invalid FCM tokens for user {UserId}", result.FailureCount, userId);
        }
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, string channelId, Dictionary<string, string>? data = null)
    {
        if (userIds is null)
            throw new ArgumentNullException(nameof(userIds));
        
        var tokens = (await _tokenRepo.GetActiveTokensUsersAsync(userIds)).AsReadOnly();
        if (tokens.Count == 0)
            return;

        var result = await _provider.SendMulticastAsync(tokens, new PushMessage()
        {
            Title = title,
            Body = body,
            Data = data ?? new(),
            ChannelId = channelId
        });
        
        if (result.FailureCount > 0)
        {
            await _tokenRepo.RemoveTokensAsync(result.FailedTokens);
            _logger.LogWarning("Removed {Count} invalid FCM tokens for users {Users}...", result.FailureCount, userIds);
        }
    }

    public async Task SendToSessionAsync(Guid sessionId, string title, string body, string channelId, Dictionary<string, string>? data = null)
    {
        var token = await _tokenRepo.GetActiveTokenBySessionAsync(sessionId);
        if(string.IsNullOrEmpty(token))
            return;
        
        var result = await _provider.SendToTokenAsync(token, new PushMessage()
        {
            Title = title,
            Body = body,
            Data = data ?? new(),
            ChannelId = channelId
        });
        
        if (result.FailureCount > 0)
        {
            await _tokenRepo.RemoveTokensAsync(result.FailedTokens);
            _logger.LogWarning("Removed {Count} invalid FCM tokens for session {Session}...", result.FailureCount, sessionId);
        }
    }
}