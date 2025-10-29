using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.SeedAdminData;

public class SeedAdminDataCommandHandler : IRequestHandler<SeedAdminDataCommand, SeedAdminDataResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedAdminDataCommandHandler> _logger;

    public SeedAdminDataCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SeedAdminDataCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SeedAdminDataResult> Handle(SeedAdminDataCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting admin data seeding...");

        var result = new SeedAdminDataResult();

        // 1. Seed Permissions
        result.PermissionsCreated = await SeedPermissions();

        // 2. Seed Role Groups
        result.RoleGroupsCreated = await SeedRoleGroups();

        // 3. Create Default Admin User
        result.AdminUserCreated = await CreateDefaultAdminUser();

        await _unitOfWork.SaveChangesAsync();

        result.Message = $"Seeded {result.PermissionsCreated} permissions, {result.RoleGroupsCreated} role groups. Admin user created: {result.AdminUserCreated}";
        _logger.LogInformation(result.Message);

        return result;
    }

    private async Task<int> SeedPermissions()
    {
        // Check if permissions already exist
        var existingPermissions = (await _unitOfWork.PermissionRepository.GetAllAsync()).ToList();
        if (existingPermissions.Any())
        {
            _logger.LogInformation("Permissions already exist. Skipping seed.");
            return 0;
        }

        var permissions = new[]
        {
            // Content Management
            new { Name = "news.view", Description = "View news articles", Category = "Content Management" },
            new { Name = "news.create", Description = "Create news articles", Category = "Content Management" },
            new { Name = "news.edit", Description = "Edit news articles", Category = "Content Management" },
            new { Name = "news.delete", Description = "Delete news articles", Category = "Content Management" },
            new { Name = "events.view", Description = "View events", Category = "Content Management" },
            new { Name = "events.create", Description = "Create events", Category = "Content Management" },
            new { Name = "events.edit", Description = "Edit events", Category = "Content Management" },
            new { Name = "events.delete", Description = "Delete events", Category = "Content Management" },

            // Organization Management
            new { Name = "departments.view", Description = "View departments", Category = "Organization Management" },
            new { Name = "departments.manage", Description = "Manage departments", Category = "Organization Management" },
            new { Name = "positions.view", Description = "View positions", Category = "Organization Management" },
            new { Name = "positions.manage", Description = "Manage positions", Category = "Organization Management" },

            // User Management
            new { Name = "users.view", Description = "View users", Category = "User Management" },
            new { Name = "users.create", Description = "Create users", Category = "User Management" },
            new { Name = "users.edit", Description = "Edit users", Category = "User Management" },
            new { Name = "users.delete", Description = "Delete users", Category = "User Management" },
            new { Name = "users.import", Description = "Import users from CSV/Excel", Category = "User Management" },

            // Admin Panel
            new { Name = "admin.access", Description = "Access admin panel", Category = "Admin Panel" },
            new { Name = "roles.view", Description = "View role groups", Category = "Admin Panel" },
            new { Name = "roles.manage", Description = "Manage role groups and permissions", Category = "Admin Panel" },
            new { Name = "audit.view", Description = "View audit logs", Category = "Admin Panel" },

            // Reports
            new { Name = "reports.view", Description = "View reports", Category = "Reports" },
            new { Name = "reports.export", Description = "Export reports", Category = "Reports" }
        };

        var count = 0;
        foreach (var perm in permissions)
        {
            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = perm.Name,
                Description = perm.Description,
                Category = perm.Category,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PermissionRepository.CreateAsync(permission);
            count++;
            _logger.LogInformation("Seeded permission: {Name}", permission.Name);
        }

        return count;
    }

    private async Task<int> SeedRoleGroups()
    {
        // Check if role groups already exist
        var existingRoleGroups = (await _unitOfWork.RoleGroupRepository.GetAllAsync()).ToList();
        if (existingRoleGroups.Any())
        {
            _logger.LogInformation("Role groups already exist. Skipping seed.");
            return 0;
        }

        // Get all permissions
        var allPermissions = (await _unitOfWork.PermissionRepository.GetAllAsync()).ToList();

        var roleGroupsData = new[]
        {
            new
            {
                Name = "Admin",
                Description = "Full system access with all permissions",
                IsSystemRole = true,
                PermissionNames = allPermissions.Select(p => p.Name).ToArray()
            },
            new
            {
                Name = "HR",
                Description = "Human Resources role with user and organization management",
                IsSystemRole = true,
                PermissionNames = new[]
                {
                    "users.view", "users.create", "users.edit", "users.import",
                    "departments.view", "departments.manage", "positions.view", "positions.manage",
                    "news.view", "news.create", "news.edit",
                    "events.view", "events.create", "events.edit",
                    "reports.view"
                }
            },
            new
            {
                Name = "Marketing",
                Description = "Marketing role with content management permissions",
                IsSystemRole = true,
                PermissionNames = new[]
                {
                    "news.view", "news.create", "news.edit", "news.delete",
                    "events.view", "events.create", "events.edit", "events.delete",
                    "users.view", "departments.view", "positions.view"
                }
            },
            new
            {
                Name = "Manager",
                Description = "Department manager with limited management permissions",
                IsSystemRole = true,
                PermissionNames = new[]
                {
                    "users.view", "departments.view", "departments.manage",
                    "positions.view", "news.view", "events.view",
                    "reports.view"
                }
            },
            new
            {
                Name = "Employee",
                Description = "Basic employee with read-only access",
                IsSystemRole = true,
                PermissionNames = new[]
                {
                    "news.view", "events.view", "users.view",
                    "departments.view", "positions.view"
                }
            }
        };

        var count = 0;
        foreach (var roleData in roleGroupsData)
        {
            var roleGroup = new RoleGroup
            {
                Id = Guid.NewGuid(),
                Name = roleData.Name,
                Description = roleData.Description,
                IsSystemRole = roleData.IsSystemRole,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RoleGroupRepository.CreateAsync(roleGroup);

            // Assign permissions to role group
            foreach (var permName in roleData.PermissionNames)
            {
                var permission = allPermissions.FirstOrDefault(p => p.Name == permName);
                if (permission != null)
                {
                    var roleGroupPermission = new RoleGroupPermission
                    {
                        RoleGroupId = roleGroup.Id,
                        PermissionId = permission.Id
                    };
                    await _unitOfWork.RoleGroupPermissionRepository.CreateAsync(roleGroupPermission);
                }
            }

            count++;
            _logger.LogInformation("Seeded role group: {Name} with {Count} permissions", roleGroup.Name, roleData.PermissionNames.Length);
        }

        return count;
    }

    private async Task<bool> CreateDefaultAdminUser()
    {
        // Check if admin user already exists
        var existingAdmin = (await _unitOfWork.UserRepository.GetAllAsync())
            .FirstOrDefault(u => u.Email == "admin@portalforge.local");

        if (existingAdmin != null)
        {
            _logger.LogInformation("Default admin user already exists");
            return false;
        }

        // Get Admin role group
        var adminRoleGroup = (await _unitOfWork.RoleGroupRepository.GetAllAsync())
            .FirstOrDefault(rg => rg.Name == "Admin");

        if (adminRoleGroup == null)
        {
            _logger.LogError("Admin role group not found. Cannot create default admin user.");
            return false;
        }

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@portalforge.local",
            FirstName = "System",
            LastName = "Administrator",
            Role = UserRole.Admin,
            Department = "IT",
            Position = "System Administrator",
            IsActive = true,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserRepository.CreateAsync(adminUser);

        // Assign Admin role group to user
        var userRoleGroup = new UserRoleGroup
        {
            UserId = adminUser.Id,
            RoleGroupId = adminRoleGroup.Id,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = null // System assignment
        };

        await _unitOfWork.UserRoleGroupRepository.CreateAsync(userRoleGroup);

        _logger.LogInformation("Created default admin user: {Email}", adminUser.Email);
        _logger.LogWarning("Default admin password is 'admin' - MUST BE CHANGED ON FIRST LOGIN");

        return true;
    }
}

