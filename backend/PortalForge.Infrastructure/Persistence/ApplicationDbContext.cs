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
    
    // Requests System
    public DbSet<RequestTemplate> RequestTemplates { get; set; }
    public DbSet<RequestTemplateField> RequestTemplateFields { get; set; }
    public DbSet<RequestApprovalStepTemplate> RequestApprovalStepTemplates { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestApprovalStep> RequestApprovalSteps { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAnswer> QuizAnswers { get; set; }

    // Notifications
    public DbSet<Notification> Notifications { get; set; }

    // Organizational Structure
    public DbSet<Department> Departments { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<VacationSchedule> VacationSchedules { get; set; }
    public DbSet<OrganizationalPermission> OrganizationalPermissions { get; set; }

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
        
        // Requests System
        modelBuilder.ApplyConfiguration(new RequestTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new RequestTemplateFieldConfiguration());
        modelBuilder.ApplyConfiguration(new RequestApprovalStepTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new RequestConfiguration());
        modelBuilder.ApplyConfiguration(new RequestApprovalStepConfiguration());
        modelBuilder.ApplyConfiguration(new QuizQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new QuizAnswerConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());

        // Organizational Structure
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new VacationScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationalPermissionConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
