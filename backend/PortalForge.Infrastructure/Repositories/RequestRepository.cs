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

    public async Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.QuizAnswers)
                    .ThenInclude(qa => qa.QuizQuestion)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.ApprovalStepTemplate)
                    .ThenInclude(ast => ast!.QuizQuestions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetBySubmitterAsync(Guid submitterId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Where(r => r.SubmittedById == submitterId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetByApproverAsync(Guid approverId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.ApprovalStepTemplate)
                    .ThenInclude(ast => ast!.QuizQuestions)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.QuizAnswers)
            .Where(r => r.ApprovalSteps.Any(aps => aps.ApproverId == approverId))
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Where(r => r.RequestTemplateId == templateId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Request?> GetByRequestNumberAsync(string requestNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
                .ThenInclude(aps => aps.Approver)
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber, cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetPendingRequestsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Requests.CountAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(Request request, CancellationToken cancellationToken = default)
    {
        await _context.Requests.AddAsync(request, cancellationToken);
        return request.Id;
    }

    public async Task UpdateAsync(Request request, CancellationToken cancellationToken = default)
    {
        _context.Requests.Update(request);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _context.Requests.FindAsync(new object[] { id }, cancellationToken);
        if (request != null)
        {
            _context.Requests.Remove(request);
        }
    }
}
