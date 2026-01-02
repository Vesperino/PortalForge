using MediatR;
using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetRequestById;

/// <summary>
/// Query to get detailed request information including comments and edit history
/// </summary>
public class GetRequestByIdQuery : IRequest<RequestDetailDto>
{
    public Guid RequestId { get; set; }

    public Guid CurrentUserId { get; set; }
}
