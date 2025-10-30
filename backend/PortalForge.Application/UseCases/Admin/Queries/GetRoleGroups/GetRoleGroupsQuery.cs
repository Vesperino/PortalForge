using MediatR;

namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

public class GetRoleGroupsQuery : IRequest<GetRoleGroupsResult>
{
    public bool IncludePermissions { get; set; } = true;
}

