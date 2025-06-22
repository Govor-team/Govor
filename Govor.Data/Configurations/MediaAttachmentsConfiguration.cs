using Govor.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class MediaAttachmentsConfiguration : IEntityTypeConfiguration<MediaAttachments>
{
    public void Configure(EntityTypeBuilder<MediaAttachments> builder)
    {
        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.FilePath)
            .IsRequired();

        builder.Property(ma => ma.MimeType)
            .IsRequired();

        builder.Property(ma => ma.EncryptedKey)
            .HasMaxLength(512); // зависит от шифра

        builder.Property(ma => ma.Type)
            .HasConversion<string>() // enum as string (e.g., "Image")
            .IsRequired();
    }
}