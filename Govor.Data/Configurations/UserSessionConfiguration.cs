using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Models.Users.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(us => us.Id);

        builder.Property(us => us.RefreshToken)
            .IsRequired();

        builder.Property(us => us.DeviceInfo)
            .HasMaxLength(256);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.CryptoSession)
            .WithOne(e => e.UserSession)
            .HasForeignKey<UserCryptoSession>(e => e.UserSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}