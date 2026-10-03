using Govor.Domain.Models;
using Govor.Domain.Models.Reactions;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class ReactionPackConfiguration : IEntityTypeConfiguration<ReactionPack>
{
    public void Configure(EntityTypeBuilder<ReactionPack> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.ShareCode).IsRequired().HasMaxLength(32);
        builder.HasIndex(p => p.ShareCode).IsUnique();
        builder.HasIndex(p => p.IsDefault).IsUnique().HasFilter("\"IsDefault\" = TRUE");
        builder.HasOne<User>().WithMany().HasForeignKey(p => p.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(p => p.Reactions).WithOne(r => r.Pack).HasForeignKey(r => r.PackId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(DefaultReactionPack.Create());
    }
}

public class ReactionItemConfiguration : IEntityTypeConfiguration<ReactionItem>
{
    public void Configure(EntityTypeBuilder<ReactionItem> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Code).IsRequired().HasMaxLength(64);
        builder.Property(r => r.Emoji).HasMaxLength(64);
        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasOne<MediaFile>().WithMany().HasForeignKey(r => r.MediaFileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(DefaultReactionPack.Items());
    }
}

public class UserReactionPackConfiguration : IEntityTypeConfiguration<UserReactionPack>
{
    public void Configure(EntityTypeBuilder<UserReactionPack> builder)
    {
        builder.HasKey(p => new { p.UserId, p.PackId });
        builder.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ReactionPack>().WithMany().HasForeignKey(p => p.PackId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChannelAllowedReactionConfiguration : IEntityTypeConfiguration<ChannelAllowedReaction>
{
    public void Configure(EntityTypeBuilder<ChannelAllowedReaction> builder)
    {
        builder.HasKey(r => new { r.GroupId, r.ReactionId });
        builder.HasOne<ChatGroup>().WithMany().HasForeignKey(r => r.GroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ReactionItem>().WithMany().HasForeignKey(r => r.ReactionId).OnDelete(DeleteBehavior.Restrict);
    }
}
