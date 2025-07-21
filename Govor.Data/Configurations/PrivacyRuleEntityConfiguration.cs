using Govor.Core.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;


public class PrivacyRuleEntityConfiguration : IEntityTypeConfiguration<PrivacyRuleEntity>
{
    public void Configure(EntityTypeBuilder<PrivacyRuleEntity> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.OwnerId).IsRequired();
        builder.Property(r => r.Area).IsRequired().HasConversion<string>();
        builder.Property(r => r.AccessType).IsRequired().HasConversion<string>();
        
        builder.Property(r => r.Whitelist).HasColumnType("jsonb"); 
        builder.Property(r => r.Blacklist).HasColumnType("jsonb");
        
        builder.HasIndex(r => r.OwnerId);
    }
}