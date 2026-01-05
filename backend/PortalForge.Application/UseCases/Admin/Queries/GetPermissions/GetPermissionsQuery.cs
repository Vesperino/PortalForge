using MediatR;

namespace PortalForge.Application.UseCases.Admin.Queries.GetPermissions;

public class GetPermissionsQuery : IRequest<GetPermissionsResult>
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

