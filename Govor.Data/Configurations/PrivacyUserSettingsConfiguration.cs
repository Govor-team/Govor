using Govor.Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class PrivacyUserSettingsConfiguration : IEntityTypeConfiguration<PrivacyUserSettings>
{
    public void Configure(EntityTypeBuilder<PrivacyUserSettings> builder)
    {
        // Primary Key
        builder.HasKey(p => p.UserId);

        builder.Property(p => p.DeletingVia)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(DeletingMessagesVia.None); // Adjust default as needed

        builder.Property(p => p.DeletingIn)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationship Configuration
        builder.HasMany(p => p.Rules)
            .WithOne(r => r.OwnerSettings)
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
