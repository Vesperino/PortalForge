using MediatR;

namespace PortalForge.Application.UseCases.Admin.Queries.GetPermissions;

public class GetPermissionsQuery : IRequest<GetPermissionsResult>
{
    public string? Category { get; set; }
}

