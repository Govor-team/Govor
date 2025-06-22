using System.Text.RegularExpressions;
using Govor.Core.Models;
using Govor.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data;

public class GovorDbContext(DbContextOptions<GovorDbContext> options) : DbContext(options)
{
    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageView> MessageViews { get; set; }
    public virtual DbSet<MessageReaction> MessageReactions { get; set; }
    public virtual DbSet<MediaAttachments> MediaAttachments { get; set; }
   
    public virtual DbSet<ChatGroup> ChatGroups { get; set; }
    public virtual DbSet<GroupMembership> GroupMemberships { get; set; }
    public virtual DbSet<GroupAdmins> GroupAdmins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MessagesConfiguration());
        modelBuilder.ApplyConfiguration(new MessageReactionConfiguration());
        modelBuilder.ApplyConfiguration(new MediaAttachmentsConfiguration());
        modelBuilder.ApplyConfiguration(new MessageViewConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}