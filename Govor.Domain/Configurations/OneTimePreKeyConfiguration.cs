using Govor.Domain.Models.Users.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class OneTimePreKeyConfiguration : IEntityTypeConfiguration<OneTimePreKey>
{
    public void Configure(EntityTypeBuilder<OneTimePreKey> builder)
    {
        // Первичный ключ
        builder.HasKey(otpk => otpk.Id);
        
        builder.HasOne(otpk => otpk.UserCryptoSession)
            .WithMany(ucs => ucs.OneTimePreKeys)
            .HasForeignKey(otpk => otpk.UserCryptoSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(otpk => otpk.PublicKey)
            .IsRequired();
        
        builder.Property(otpk => otpk.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(otpk => otpk.UploadedAt)
            .IsRequired();
        
        builder.HasIndex(otpk => new { otpk.UserCryptoSessionId, otpk.IsUsed });
        builder.HasIndex(otpk => otpk.UploadedAt);
    }
}