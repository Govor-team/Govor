using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class MessageViewConfiguration : IEntityTypeConfiguration<MessageView>
{
    public void Configure(EntityTypeBuilder<MessageView> builder)
    {
        builder.HasKey(mv => mv.Id);

        builder.HasIndex(mv => new { mv.MessageId, mv.UserId }).IsUnique();

        builder.Property(mv => mv.ViewedAt)
            .IsRequired();
    }
}