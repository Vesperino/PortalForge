using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Queries.GetAllRequests;

/// <summary>
/// Query to get all requests for Admin/HR users.
/// Implements IRequireAuthorization to enforce Admin or HR role.
/// </summary>
public class GetAllRequestsQuery : IRequest<GetAllRequestsResult>, IRequireAuthorization
{
    public Guid CurrentUserId { get; set; }

    public string[] AllowedRoles => new[] { "Admin", "HR" };
}
