using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
    public void Configure(EntityTypeBuilder<GroupMembership> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.GroupId).IsRequired();
        builder.Property(e => e.InvitationId).IsRequired(false);

        builder.HasOne(e => e.ChatGroup)          
            .WithMany()                          
            .HasForeignKey(e => e.GroupId)       
            .OnDelete(DeleteBehavior.Cascade);    

        // Optional: настройка связи с GroupInvitation
        builder.HasOne<GroupInvitation>()
            .WithMany()
            .HasForeignKey(e => e.InvitationId)
            .OnDelete(DeleteBehavior.SetNull);
        
    }
}