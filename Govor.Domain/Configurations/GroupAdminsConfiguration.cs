using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class GroupAdminsConfiguration : IEntityTypeConfiguration<GroupAdmins>
{
    public void Configure(EntityTypeBuilder<GroupAdmins> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.GroupId).IsRequired();
    }
}