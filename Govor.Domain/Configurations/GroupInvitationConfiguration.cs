using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class GroupInvitationConfiguration : IEntityTypeConfiguration<GroupInvitation>
{
    public void Configure(EntityTypeBuilder<GroupInvitation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvitationCode).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.EndDate).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        
        builder.HasOne(e => e.UserMaker)
            .WithMany() 
            .HasForeignKey(e => e.UserMakerId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Ignore(e => e.GroupMemberships);
    }
}