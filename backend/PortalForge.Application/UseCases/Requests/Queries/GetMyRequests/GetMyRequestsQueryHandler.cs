using System.Text.Json;
using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Queries.GetMyRequests;

public class GetMyRequestsQueryHandler 
    : IRequestHandler<GetMyRequestsQuery, GetMyRequestsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMyRequestsResult> Handle(
        GetMyRequestsQuery request, 
        CancellationToken cancellationToken)
    {
        // Get all requests for the user first
        var allUserRequests = await _unitOfWork.RequestRepository.GetBySubmitterAsync(request.UserId);
        
        // Apply filters
        var filteredRequests = ApplyFilters(allUserRequests, request);
        
        // Apply sorting
        var sortedRequests = ApplySorting(filteredRequests, request);
        
        // Calculate pagination
        var totalCount = sortedRequests.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var skip = (request.PageNumber - 1) * request.PageSize;
        var pagedRequests = sortedRequests.Skip(skip).Take(request.PageSize);

        var requestDtos = pagedRequests.Select(r => new RequestDto
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestTemplateId = r.RequestTemplateId,
            RequestTemplateName = r.RequestTemplate?.Name ?? string.Empty,
            RequestTemplateIcon = r.RequestTemplate?.Icon ?? string.Empty,
            SubmittedById = r.SubmittedById,
            SubmittedByName = r.SubmittedBy?.FullName ?? string.Empty,
            SubmittedAt = r.SubmittedAt,
            Priority = r.Priority.ToString(),
            FormData = r.FormData,
            Status = r.Status.ToString(),
            CompletedAt = r.CompletedAt,
            ApprovalSteps = r.ApprovalSteps.Select(aps => new RequestApprovalStepDto
            {
                Id = aps.Id,
                StepOrder = aps.StepOrder,
                ApproverId = aps.ApproverId,
                ApproverName = aps.Approver?.FullName ?? string.Empty,
                Status = aps.Status.ToString(),
                StartedAt = aps.StartedAt,
                FinishedAt = aps.FinishedAt,
                Comment = aps.Comment,
                RequiresQuiz = aps.RequiresQuiz,
                QuizScore = aps.QuizScore,
                QuizPassed = aps.QuizPassed
            }).ToList()
        }).ToList();

        // Generate filter summary
        var filterSummary = GenerateFilterSummary(allUserRequests);

        return new GetMyRequestsResult
        {
            Requests = requestDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.PageNumber < totalPages,
            HasPreviousPage = request.PageNumber > 1,
            FilterSummary = filterSummary
        };
    }

    private IEnumerable<Domain.Entities.Request> ApplyFilters(
        IEnumerable<Domain.Entities.Request> requests, 
        GetMyRequestsQuery query)
    {
        var filtered = requests.AsEnumerable();

        // Search term filter - searches across multiple fields
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(r => 
                r.RequestNumber.ToLowerInvariant().Contains(searchTerm) ||
                (r.RequestTemplate?.Name?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                SearchInFormData(r.FormData, searchTerm) ||
                (r.ServiceNotes?.ToLowerInvariant().Contains(searchTerm) ?? false));
        }

        // Status filter
        if (query.StatusFilter?.Any() == true)
        {
            filtered = filtered.Where(r => query.StatusFilter.Contains(r.Status));
        }

        // Priority filter
        if (query.PriorityFilter?.Any() == true)
        {
            filtered = filtered.Where(r => query.PriorityFilter.Contains(r.Priority));
        }

        // Template ID filter
        if (query.TemplateIdFilter?.Any() == true)
        {
            filtered = filtered.Where(r => query.TemplateIdFilter.Contains(r.RequestTemplateId));
        }

        // Leave type filter
        if (query.LeaveTypeFilter?.Any() == true)
        {
            filtered = filtered.Where(r => r.LeaveType.HasValue && query.LeaveTypeFilter.Contains(r.LeaveType.Value));
        }

        // Date filters
        if (query.SubmittedAfter.HasValue)
        {
            filtered = filtered.Where(r => r.SubmittedAt >= query.SubmittedAfter.Value);
        }

        if (query.SubmittedBefore.HasValue)
        {
            filtered = filtered.Where(r => r.SubmittedAt <= query.SubmittedBefore.Value);
        }

        if (query.CompletedAfter.HasValue)
        {
            filtered = filtered.Where(r => r.CompletedAt.HasValue && r.CompletedAt >= query.CompletedAfter.Value);
        }

        if (query.CompletedBefore.HasValue)
        {
            filtered = filtered.Where(r => r.CompletedAt.HasValue && r.CompletedAt <= query.CompletedBefore.Value);
        }

        // Tags filter
        if (query.TagsFilter?.Any() == true)
        {
            filtered = filtered.Where(r => HasMatchingTags(r.Tags, query.TagsFilter));
        }

        // Cloned filter
        if (query.IsCloned.HasValue)
        {
            filtered = filtered.Where(r => r.ClonedFromId.HasValue == query.IsCloned.Value);
        }

        // Template filter
        if (query.IsTemplate.HasValue)
        {
            filtered = filtered.Where(r => r.IsTemplate == query.IsTemplate.Value);
        }

        return filtered;
    }

    private IEnumerable<Domain.Entities.Request> ApplySorting(
        IEnumerable<Domain.Entities.Request> requests, 
        GetMyRequestsQuery query)
    {
        var sorted = requests.AsEnumerable();

        // Primary sort
        sorted = query.SortBy.ToLowerInvariant() switch
        {
            "submittedat" => query.SortDirection.ToUpperInvariant() == "ASC" 
                ? sorted.OrderBy(r => r.SubmittedAt)
                : sorted.OrderByDescending(r => r.SubmittedAt),
            "priority" => query.SortDirection.ToUpperInvariant() == "ASC"
                ? sorted.OrderBy(r => r.Priority)
                : sorted.OrderByDescending(r => r.Priority),
            "status" => query.SortDirection.ToUpperInvariant() == "ASC"
                ? sorted.OrderBy(r => r.Status)
                : sorted.OrderByDescending(r => r.Status),
            "requestnumber" => query.SortDirection.ToUpperInvariant() == "ASC"
                ? sorted.OrderBy(r => r.RequestNumber)
                : sorted.OrderByDescending(r => r.RequestNumber),
            "completedat" => query.SortDirection.ToUpperInvariant() == "ASC"
                ? sorted.OrderBy(r => r.CompletedAt ?? DateTime.MaxValue)
                : sorted.OrderByDescending(r => r.CompletedAt ?? DateTime.MinValue),
            _ => sorted.OrderByDescending(r => r.SubmittedAt) // Default sort
        };

        // Secondary sorts
        if (query.SecondarySortBy?.Any() == true)
        {
            var orderedSorted = sorted as IOrderedEnumerable<Domain.Entities.Request>;
            foreach (var secondarySort in query.SecondarySortBy)
            {
                orderedSorted = secondarySort.ToLowerInvariant() switch
                {
                    "priority" => orderedSorted?.ThenBy(r => r.Priority),
                    "status" => orderedSorted?.ThenBy(r => r.Status),
                    "requestnumber" => orderedSorted?.ThenBy(r => r.RequestNumber),
                    _ => orderedSorted
                };
            }
            sorted = orderedSorted ?? sorted;
        }

        return sorted;
    }

    private bool SearchInFormData(string formData, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(formData))
            return false;

        try
        {
            var formDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(formData);
            if (formDict == null)
                return false;

            return formDict.Values.Any(value => 
            {
                if (value.ValueKind == JsonValueKind.String)
                {
                    var stringValue = value.GetString();
                    return !string.IsNullOrWhiteSpace(stringValue) && 
                           stringValue.ToLowerInvariant().Contains(searchTerm);
                }
                return false;
            });
        }
        catch
        {
            return false;
        }
    }

    private bool HasMatchingTags(string? tagsJson, List<string> filterTags)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
            return false;

        try
        {
            var tags = JsonSerializer.Deserialize<List<string>>(tagsJson);
            if (tags == null)
                return false;

            return filterTags.Any(filterTag => 
                tags.Any(tag => tag.ToLowerInvariant().Contains(filterTag.ToLowerInvariant())));
        }
        catch
        {
            return false;
        }
    }

    private FilterSummary GenerateFilterSummary(IEnumerable<Domain.Entities.Request> allRequests)
    {
        return new FilterSummary
        {
            TotalRequests = allRequests.Count(),
            DraftRequests = allRequests.Count(r => r.Status == RequestStatus.Draft),
            InReviewRequests = allRequests.Count(r => r.Status == RequestStatus.InReview),
            ApprovedRequests = allRequests.Count(r => r.Status == RequestStatus.Approved),
            RejectedRequests = allRequests.Count(r => r.Status == RequestStatus.Rejected),
            ClonedRequests = allRequests.Count(r => r.ClonedFromId.HasValue),
            TemplateRequests = allRequests.Count(r => r.IsTemplate)
        };
    }
}

