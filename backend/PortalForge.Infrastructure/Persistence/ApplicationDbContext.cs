using Microsoft.EntityFrameworkCore;
using PortalForge.Domain.Entities;
using PortalForge.Infrastructure.Persistence.Configurations;

namespace PortalForge.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RoleGroup> RoleGroups { get; set; }
    public DbSet<RoleGroupPermission> RoleGroupPermissions { get; set; }
    public DbSet<UserRoleGroup> UserRoleGroups { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Hashtag> Hashtags { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<CachedLocation> CachedLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new NewsConfiguration());
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RoleGroupConfiguration());
        modelBuilder.ApplyConfiguration(new RoleGroupPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleGroupConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new HashtagConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingConfiguration());
        modelBuilder.ApplyConfiguration(new CachedLocationConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
