using Govor.Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class UserPushTokenConfiguration : IEntityTypeConfiguration<UserPushToken>
{
    public void Configure(EntityTypeBuilder<UserPushToken> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.HasIndex(x => x.Token)
            .IsUnique();
        
        builder.HasIndex(x => x.UserSessionId)
            .IsUnique();
        
        builder.HasIndex(x => new { x.UserId, x.IsActive });
        
        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);
    }
}