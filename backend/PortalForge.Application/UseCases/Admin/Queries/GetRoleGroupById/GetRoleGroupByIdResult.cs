using PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroupById;

public class GetRoleGroupByIdResult
{
    public RoleGroupDto RoleGroup { get; set; } = new();
}

