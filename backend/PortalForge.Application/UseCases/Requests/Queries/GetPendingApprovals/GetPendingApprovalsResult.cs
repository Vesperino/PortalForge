using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetPendingApprovals;

public class GetPendingApprovalsResult
{
    public List<RequestDto> Requests { get; set; } = new();
}

