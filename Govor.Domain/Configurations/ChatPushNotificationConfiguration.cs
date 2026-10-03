using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class ChatPushNotificationConfiguration : IEntityTypeConfiguration<ChatPushNotification>
{
    public void Configure(EntityTypeBuilder<ChatPushNotification> builder)
    {
        builder.HasKey(n => n.MessageId);
        builder.HasIndex(n => n.NextAttemptAt);
        builder.HasOne<Message>().WithMany().HasForeignKey(n => n.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
