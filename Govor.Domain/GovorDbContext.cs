using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Users;
using Govor.Domain.Models.Users.Crypto;
using Govor.Domain.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Govor.Domain;

public class GovorDbContext(DbContextOptions<GovorDbContext> options) : DbContext(options)
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserSession> UserSessions { get; set; }
    public virtual DbSet<UserPushToken>  UserPushTokens { get; set; }
    public virtual DbSet<UserCryptoSession> UserCryptoSessions { get; set; }
    public virtual DbSet<SignedPreKey> SignedPreKeys { get; set; }
    public virtual DbSet<OneTimePreKey> OneTimePreKeys { get; set; }
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
    public virtual DbSet<GroupInvitation> GroupInvitations { get; set; }
    public virtual DbSet<GroupMembership> GroupMemberships { get; set; }
    public virtual DbSet<GroupAdmins> GroupAdmins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ApplyConfiguration(new FriendshipConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationConfiguration());
        modelBuilder.ApplyConfiguration(new AdminConfiguration());
        modelBuilder.ApplyConfiguration(new MessagesConfiguration());
        modelBuilder.ApplyConfiguration(new MessageReactionConfiguration());
        modelBuilder.ApplyConfiguration(new MediaAttachmentsConfiguration());
        modelBuilder.ApplyConfiguration(new MessageViewConfiguration());
        modelBuilder.ApplyConfiguration(new MediaFileConfiguration());
        modelBuilder.ApplyConfiguration(new ChatGroupConfigurator());
        modelBuilder.ApplyConfiguration(new GroupInvitationConfiguration());
        modelBuilder.ApplyConfiguration(new GroupMembershipConfiguration());
        modelBuilder.ApplyConfiguration(new GroupAdminsConfiguration());

        modelBuilder.ApplyConfiguration(new OneTimePreKeyConfiguration());
        modelBuilder.ApplyConfiguration(new UserCryptoSessionConfiguration());
        modelBuilder.ApplyConfiguration(new SignedPreKeyConfiguration());
        modelBuilder.ApplyConfiguration(new PrivateChatsConfiguration());
        modelBuilder.ApplyConfiguration(new UserPushTokenConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}