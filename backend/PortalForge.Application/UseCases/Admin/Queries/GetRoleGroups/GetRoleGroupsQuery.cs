using MediatR;

namespace PortalForge.Application.UseCases.Admin.Queries.GetRoleGroups;

public class GetRoleGroupsQuery : IRequest<GetRoleGroupsResult>
{
    public string? SearchTerm { get; set; }
    public bool? IsSystemRole { get; set; }
    public bool IncludePermissions { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

