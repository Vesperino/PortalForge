namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

public class GetRoleGroupsResult
{
    public List<RoleGroupDto> RoleGroups { get; set; } = new();
}

public class RoleGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
    public int UserCount { get; set; }
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

