using Govor.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
    public void Configure(EntityTypeBuilder<GroupMembership> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.GroupId).IsRequired();
        builder.Property(e => e.InvitationId).IsRequired();

        // Optional: можно добавить навигацию к GroupInvitation
        builder.HasOne<GroupInvitation>()
            .WithMany()
            .HasForeignKey(e => e.InvitationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}