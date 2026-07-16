using Govor.Domain.Models.Users;
using Govor.Domain.Models.Users.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(us => us.Id);
        
        builder.HasIndex(us => us.UserId);
        
        builder.HasIndex(s => s.RefreshTokenHash)
            .IsUnique(); 
        
        builder.Property(us => us.RefreshTokenHash)
            .IsRequired();

        builder.Property(us => us.DeviceInfo)
            .HasMaxLength(256);
        
        builder.HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.HasOne(e => e.CryptoSession)
            .WithOne(e => e.UserSession)
            .HasForeignKey<UserCryptoSession>(e => e.UserSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}