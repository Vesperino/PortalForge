using MediatR;

namespace PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;

public class GetMyRequestsQuery : IRequest<GetMyRequestsResult>
{
    public Guid UserId { get; set; }
}

