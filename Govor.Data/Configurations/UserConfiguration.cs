using Govor.Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(u => u.Invite)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.InviteId);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(128);
    }
}