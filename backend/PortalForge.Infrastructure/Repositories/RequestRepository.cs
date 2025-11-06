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
                    .ThenInclude(qa => qa.QuizQuestion)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.ApprovalStepTemplate)
                    .ThenInclude(ast => ast.QuizQuestions)
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
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.ApprovalStepTemplate)
                    .ThenInclude(ast => ast.QuizQuestions)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(aps => aps.QuizAnswers)
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

    public async Task DeleteAsync(Guid id)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request != null)
        {
            _context.Requests.Remove(request);
        }
    }
}

