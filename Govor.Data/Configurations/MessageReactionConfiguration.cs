using Govor.Core.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.MessageId, r.UserId }).IsUnique(); // Одна реакция от одного юзера

        builder.Property(r => r.ReactionCode)
            .IsRequired()
            .HasMaxLength(64); // можно увеличить для кастомных эмодзи

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}