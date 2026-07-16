using Govor.Application.Interfaces.PushNotifications.Models;
using Govor.Application.PushNotifications.Providers;
using Govor.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Govor.Application.PushNotifications;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushNotificationProvider _provider;
    private readonly IPushTokenService _tokenService; 
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IPushNotificationProvider provider,
        IPushTokenService tokenService,
        ILogger<PushNotificationService> logger)
    {
        _provider = provider;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result> SendToUserAsync(Guid userId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null)
    {
        var resultTokens = await _tokenService.GetStringsActiveTokensAsync(userId);
        if (resultTokens.IsFailure)
            return Result.Failure(resultTokens.Error);

        var tokens = resultTokens.Value;
        if (tokens.Count == 0)
        {
            _logger.LogInformation("No active push tokens for user {UserId}", userId);
            return Result.Success();
        }

        return await SendMulticastInternalAsync(tokens, title, body, channelId, tag, data, userId.ToString());
    }

    public async Task<Result> SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null)
    {
        if (userIds == null || !userIds.Any())
            return Result.Failure(new Error("Push.InvalidArgs", "User IDs collection cannot be empty."));

        var tokensResult = await _tokenService.GetUsersStringsActiveTokensAsync(userIds);
        if (tokensResult.IsFailure)
            return Result.Failure(tokensResult.Error);
        
        var tokens = tokensResult.Value;
        if (tokens.Count == 0)
            return Result.Success();

        return await SendMulticastInternalAsync(tokens, title, body, channelId, tag, data, "bulk_request");
    }

    public async Task<Result> SendToSessionAsync(Guid sessionId, string title, string body, string channelId, string tag = "", Dictionary<string, string>? data = null)
    {
        var tokenResult = await _tokenService.GetActiveTokenBySessionAsync(sessionId);
        if (tokenResult.IsFailure)
            return Result.Failure(tokenResult.Error);

        var token = tokenResult.Value;
        if (string.IsNullOrEmpty(token))
            return Result.Success();

        try
        {
            var message = CreateMessage(title, body, channelId, tag, data);
            var result = await _provider.SendToTokenAsync(token, message);
            
            if (result.FailureCount > 0)
            {
                await _tokenService.RemoveTokensAsync(result.FailedTokens);
                _logger.LogWarning("Removed {Count} invalid FCM tokens for session {SessionId}", result.FailureCount, sessionId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send single push notification to session {SessionId}", sessionId);
            return Result.Failure(ex);
        }
    }
    
    private async Task<Result> SendMulticastInternalAsync(List<string> tokens, string title, string body, string channelId, string tag, Dictionary<string, string>? data, string targetInfo)
    {
        try
        {
            var message = CreateMessage(title, body, channelId, tag, data);
            var result = await _provider.SendMulticastAsync(tokens, message);

            if (result.FailureCount > 0)
            {
                await _tokenService.RemoveTokensAsync(result.FailedTokens);
                _logger.LogWarning("Removed {Count} invalid FCM tokens for target: {Target}", result.FailureCount, targetInfo);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during push notification multicast sending to {Target}", targetInfo);
            return Result.Failure(ex);
        }
    }

    private static PushMessage CreateMessage(string title, string body, string channelId, string tag, Dictionary<string, string>? data)
    {
        return new PushMessage
        {
            Title = title,
            Body = body,
            Data = data ?? new(),
            Tag = tag,
            ChannelId = channelId
        };
    }
}