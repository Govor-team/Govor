using Govor.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Data.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.HasMany(i => i.Users)
            .WithOne(u => u.Invite)
            .HasForeignKey(u => u.InviteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}