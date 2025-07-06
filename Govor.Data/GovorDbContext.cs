using System.Text.RegularExpressions;
using Govor.Core.Models;
using Govor.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data;

public class GovorDbContext(DbContextOptions<GovorDbContext> options) : DbContext(options)
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Friendship> Friendships { get; set; }
    public virtual DbSet<PrivateChat> PrivateChats { get; set; }
    public virtual DbSet<Admin> Admins { get; set; }
    
    public virtual DbSet<Invitation> Invitations { get; set; }
    
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageView> MessageViews { get; set; }
    public virtual DbSet<MessageReaction> MessageReactions { get; set; }
    public virtual DbSet<MediaAttachments> MediaAttachments { get; set; }
    public virtual DbSet<MediaFile> MediaFiles { get; set; }
   
    public virtual DbSet<ChatGroup> ChatGroups { get; set; }
    public virtual DbSet<GroupMembership> GroupMemberships { get; set; }
    public virtual DbSet<GroupAdmins> GroupAdmins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FriendshipConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationConfiguration());
        modelBuilder.ApplyConfiguration(new AdminConfiguration());
        modelBuilder.ApplyConfiguration(new MessagesConfiguration());
        modelBuilder.ApplyConfiguration(new MessageReactionConfiguration());
        modelBuilder.ApplyConfiguration(new MediaAttachmentsConfiguration());
        modelBuilder.ApplyConfiguration(new MessageViewConfiguration());
        modelBuilder.ApplyConfiguration(new MediaFileConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}