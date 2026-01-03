using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetAllRequests;

public class GetAllRequestsResult
{
    public List<RequestDto> Requests { get; set; } = new();
    public int TotalCount { get; set; }
}
