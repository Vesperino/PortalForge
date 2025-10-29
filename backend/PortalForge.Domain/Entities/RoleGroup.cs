namespace PortalForge.Domain.Entities;

public class RoleGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<RoleGroupPermission> RoleGroupPermissions { get; set; } = new List<RoleGroupPermission>();
    public ICollection<UserRoleGroup> UserRoleGroups { get; set; } = new List<UserRoleGroup>();
}

