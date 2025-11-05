using PortalForge.Application.UseCases.Requests.DTOs;

namespace PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;

public class GetMyRequestsResult
{
    public List<RequestDto> Requests { get; set; } = new();
    
    // Pagination metadata
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    
    // Filter summary
    public FilterSummary FilterSummary { get; set; } = new();
}

public class FilterSummary
{
    public int TotalRequests { get; set; }
    public int DraftRequests { get; set; }
    public int InReviewRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int ClonedRequests { get; set; }
    public int TemplateRequests { get; set; }
}

