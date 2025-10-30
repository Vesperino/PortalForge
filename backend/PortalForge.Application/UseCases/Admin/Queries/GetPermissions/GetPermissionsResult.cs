using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

namespace PortalForge.Application.UseCases.Admin.Queries.GetPermissions;

public class GetPermissionsResult
{
    public List<PermissionDto> Permissions { get; set; } = new();
    public Dictionary<string, List<PermissionDto>> PermissionsByCategory { get; set; } = new();
}

