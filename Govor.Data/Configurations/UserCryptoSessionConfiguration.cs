using Govor.Core.Models.Users.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class UserCryptoSessionConfiguration : IEntityTypeConfiguration<UserCryptoSession>
{
    public void Configure(EntityTypeBuilder<UserCryptoSession> builder)
    {
        builder.HasKey(ucs => ucs.Id);
        
        builder.HasOne(ucs => ucs.UserSession)
            .WithOne(us => us.CryptoSession)
            .HasForeignKey<UserCryptoSession>(ucs => ucs.UserSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ucs => ucs.SignedPreKey)
            .WithOne(spk => spk.UserCryptoSession)
            .HasForeignKey<SignedPreKey>(spk => spk.UserCryptoSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(ucs => ucs.OneTimePreKeys)
            .WithOne(otpk => otpk.UserCryptoSession)
            .HasForeignKey(otpk => otpk.UserCryptoSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(ucs => ucs.UserSessionId)
            .IsUnique();
        
        builder.Property(ucs => ucs.PublicIdentityKey)
            .IsRequired();
    }
}