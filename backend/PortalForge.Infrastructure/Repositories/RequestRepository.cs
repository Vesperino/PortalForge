using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly ApplicationDbContext _context;

    public RequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Request?> GetByIdAsync(Guid id)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.QuizAnswers)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Request>> GetAllAsync()
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetBySubmitterAsync(Guid submitterId)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Where(r => r.SubmittedById == submitterId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetByApproverAsync(Guid approverId)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Where(r => r.ApprovalSteps.Any(aps => aps.ApproverId == approverId))
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetByTemplateIdAsync(Guid templateId)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Where(r => r.RequestTemplateId == templateId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Request?> GetByRequestNumberAsync(string requestNumber)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber);
    }

    public async Task<IEnumerable<Request>> GetPendingRequestsByUserAsync(Guid userId)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Where(r => r.SubmittedById == userId &&
                       (r.Status == RequestStatus.Draft ||
                        r.Status == RequestStatus.InReview ||
                        r.Status == RequestStatus.AwaitingSurvey))
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Request request)
    {
        await _context.Requests.AddAsync(request);
        return request.Id;
    }

    public async Task UpdateAsync(Request request)
    {
        _context.Requests.Update(request);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Request>> GetByServiceCategoryAsync(string serviceCategory)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Where(r => r.ServiceCategory == serviceCategory)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetTemplateRequestsAsync(Guid? userId = null)
    {
        var query = _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Where(r => r.IsTemplate);

        if (userId.HasValue)
        {
            query = query.Where(r => r.SubmittedById == userId.Value);
        }

        return await query
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetClonedRequestsAsync(Guid originalRequestId)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Where(r => r.ClonedFromId == originalRequestId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetByTagsAsync(List<string> tags)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Where(r => r.Tags != null && tags.Any(tag => r.Tags.Contains(tag)))
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> SearchRequestsAsync(
        string? searchTerm = null,
        List<RequestStatus>? statusFilter = null,
        List<Guid>? templateIds = null,
        DateTime? submittedAfter = null,
        DateTime? submittedBefore = null)
    {
        var query = _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .AsQueryable();

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(r => 
                r.RequestNumber.ToLower().Contains(lowerSearchTerm) ||
                (r.RequestTemplate != null && r.RequestTemplate.Name.ToLower().Contains(lowerSearchTerm)) ||
                r.FormData.ToLower().Contains(lowerSearchTerm) ||
                (r.ServiceNotes != null && r.ServiceNotes.ToLower().Contains(lowerSearchTerm)));
        }

        // Apply status filter
        if (statusFilter?.Any() == true)
        {
            query = query.Where(r => statusFilter.Contains(r.Status));
        }

        // Apply template IDs filter
        if (templateIds?.Any() == true)
        {
            query = query.Where(r => templateIds.Contains(r.RequestTemplateId));
        }

        // Apply date filters
        if (submittedAfter.HasValue)
        {
            query = query.Where(r => r.SubmittedAt >= submittedAfter.Value);
        }

        if (submittedBefore.HasValue)
        {
            query = query.Where(r => r.SubmittedAt <= submittedBefore.Value);
        }

        return await query
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request != null)
        {
            _context.Requests.Remove(request);
        }
    }
}

