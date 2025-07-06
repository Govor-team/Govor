using Govor.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class MediaAttachmentsConfiguration : IEntityTypeConfiguration<MediaAttachments>
{
    public void Configure(EntityTypeBuilder<MediaAttachments> builder)
    {
        builder.HasKey(ma => ma.Id);

        builder.HasOne(ma => ma.Message)
            .WithMany(m => m.MediaAttachments)
            .HasForeignKey(ma => ma.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ma => ma.MediaFile)
            .WithMany()
            .HasForeignKey(ma => ma.MediaFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}