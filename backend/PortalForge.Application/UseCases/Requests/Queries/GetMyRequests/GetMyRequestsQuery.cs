using MediatR;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;

public class GetMyRequestsQuery : IRequest<GetMyRequestsResult>
{
    public Guid UserId { get; set; }
    
    // Advanced filtering options
    public string? SearchTerm { get; set; }
    public List<RequestStatus>? StatusFilter { get; set; }
    public List<RequestPriority>? PriorityFilter { get; set; }
    public List<Guid>? TemplateIdFilter { get; set; }
    public List<LeaveType>? LeaveTypeFilter { get; set; }
    public DateTime? SubmittedAfter { get; set; }
    public DateTime? SubmittedBefore { get; set; }
    public DateTime? CompletedAfter { get; set; }
    public DateTime? CompletedBefore { get; set; }
    public List<string>? TagsFilter { get; set; }
    public bool? IsCloned { get; set; }
    public bool? IsTemplate { get; set; }
    
    // Sorting options
    public string SortBy { get; set; } = "SubmittedAt"; // SubmittedAt, Priority, Status, RequestNumber, CompletedAt
    public string SortDirection { get; set; } = "DESC"; // ASC, DESC
    public List<string>? SecondarySortBy { get; set; } // Additional sort criteria
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

