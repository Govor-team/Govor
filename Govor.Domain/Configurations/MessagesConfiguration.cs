using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class MessagesConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        
        // Просто индекс, без unique
        builder.HasIndex(m => m.RecipientId);

        builder.HasMany(m => m.Reactions)
            .WithOne(r => r.Message)
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.MediaAttachments)
            .WithOne(ma => ma.Message)
            .HasForeignKey(ma => ma.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.MessageViews)
            .WithOne()
            .HasForeignKey(mv => mv.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(m => m.EncryptedContent)
            .IsRequired();
    }
}