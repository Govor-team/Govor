using Govor.Application.Infrastructure.Common;
using Govor.Contracts.Responses.SignalR;
using Govor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Hubs.Infrastructure;

public class ChatPushDispatcher(GovorDbContext context, IChatNotificationService notifier,
    INowDateTimeProvider dateTimeProvider)
{
    public async Task DispatchAsync(CancellationToken cancellationToken = default)
    {
        var now = dateTimeProvider.Now;
        var pending = await context.ChatPushNotifications.AsNoTracking()
            .Where(n => n.NextAttemptAt <= now).OrderBy(n => n.NextAttemptAt)
            .Take(20).Select(n => n.MessageId).ToListAsync(cancellationToken);

        foreach (var id in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Conditional update claims a two-minute lease, including across server instances.
            // An interrupted worker leaves a retryable row rather than losing the notification.
            var claimed = await context.ChatPushNotifications
                .Where(n => n.MessageId == id && n.NextAttemptAt <= now)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.NextAttemptAt, now.AddMinutes(2))
                    .SetProperty(n => n.Attempts, n => n.Attempts + 1), cancellationToken);
            if (claimed == 0)
                continue;

            var message = await context.Messages.AsNoTracking()
                .Where(m => m.Id == id).Select(m => new UserMessageResponse
                {
                    MessageId = m.Id, SenderId = m.SenderId, RecipientId = m.RecipientId,
                    RecipientType = m.RecipientType, EncryptedContent = m.EncryptedContent,
                    SentAt = m.SentAt, ReplyToMessageId = m.ReplyToMessageId
                }).FirstOrDefaultAsync(cancellationToken);
            var delivered = message is null || await notifier.DeliverPushAsync(message);
            if (delivered)
                await context.ChatPushNotifications.Where(n => n.MessageId == id)
                    .ExecuteDeleteAsync(cancellationToken);
            else
            {
                var attempts = await context.ChatPushNotifications.Where(n => n.MessageId == id)
                    .Select(n => n.Attempts).FirstOrDefaultAsync(cancellationToken);
                var retryAt = dateTimeProvider.Now.AddSeconds(Math.Min(300, Math.Pow(2, Math.Min(attempts, 9))));
                await context.ChatPushNotifications.Where(n => n.MessageId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.NextAttemptAt, retryAt), cancellationToken);
            }
        }
    }
}
