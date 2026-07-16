using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.HasKey(ma => ma.Id);

        builder.Property(mf => mf.Url)
            .IsRequired();

        builder.Property(mf => mf.MediaType)
            .HasConversion<string>() // enum as string (e.g., "Image")
            .IsRequired();

        builder.Property(ma => ma.MineType)
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(mf => mf.DateCreated)
            .IsRequired();
    }
}