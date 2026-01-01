namespace PortalForge.Api.DTOs.Requests.RoleGroups;

public sealed class CreateRoleGroupRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<Guid> PermissionIds { get; init; } = new();
}
