using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Govor.Domain.Configurations;

public class ServerCommunitySettingsConfiguration : IEntityTypeConfiguration<ServerCommunitySettings>
{
    public void Configure(EntityTypeBuilder<ServerCommunitySettings> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.HasOne<ChatGroup>().WithMany().HasForeignKey(s => s.RequiredChannelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasData(new ServerCommunitySettings { Id = 1, AllowLeave = true });
    }
}
