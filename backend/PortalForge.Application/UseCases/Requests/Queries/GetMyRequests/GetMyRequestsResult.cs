using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;

public class GetMyRequestsResult
{
    public List<RequestDto> Requests { get; set; } = new();
}

