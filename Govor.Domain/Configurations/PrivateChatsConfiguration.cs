using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class PrivateChatsConfiguration : IEntityTypeConfiguration<PrivateChat>
{
    public void Configure(EntityTypeBuilder<PrivateChat> builder)
    {
        builder.HasKey(us => us.Id);
    }
}