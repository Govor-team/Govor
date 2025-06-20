using System.Text.RegularExpressions;
using Govor.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data;

public class GovorDbContext(DbContextOptions<GovorDbContext> options) : DbContext(options)
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ChatGroup> ChatGroups { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<GroupMembership> GroupMemberships { get; set; }
    public virtual DbSet<GroupAdmins> GroupAdmins { get; set; }
}