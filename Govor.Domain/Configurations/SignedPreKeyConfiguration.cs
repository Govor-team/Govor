using Govor.Domain.Models.Users.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class SignedPreKeyConfiguration : IEntityTypeConfiguration<SignedPreKey>
{
    public void Configure(EntityTypeBuilder<SignedPreKey> builder)
    {
        builder.HasKey(spk => spk.Id);

        builder.HasOne(spk => spk.UserCryptoSession)
            .WithOne(ucs => ucs.SignedPreKey)
            .HasForeignKey<SignedPreKey>(spk => spk.UserCryptoSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(spk => spk.PublicSignedPreKey)
            .IsRequired();
        builder.Property(spk => spk.SignedPreKeySignature)
            .IsRequired();

        builder.HasIndex(spk => spk.UserCryptoSessionId)
            .IsUnique();
    }
}