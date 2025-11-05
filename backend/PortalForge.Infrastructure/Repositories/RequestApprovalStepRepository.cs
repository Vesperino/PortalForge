using Microsoft.EntityFrameworkCore;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Repositories;

public class RequestApprovalStepRepository : IRequestApprovalStepRepository
{
    private readonly ApplicationDbContext _context;

    public RequestApprovalStepRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestApprovalStep?> GetByIdAsync(Guid id)
    {
        return await _context.RequestApprovalSteps
            .Include(s => s.Request)
            .Include(s => s.Approver)
            .Include(s => s.AssignedToUser)
            .Include(s => s.StepTemplate)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<RequestApprovalStep>> GetAllAsync()
    {
        return await _context.RequestApprovalSteps
            .Include(s => s.Request)
            .Include(s => s.Approver)
            .Include(s => s.AssignedToUser)
            .Include(s => s.StepTemplate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestApprovalStep>> GetByRequestIdAsync(Guid requestId)
    {
        return await _context.RequestApprovalSteps
            .Include(s => s.Approver)
            .Include(s => s.AssignedToUser)
            .Include(s => s.StepTemplate)
            .Where(s => s.RequestId == requestId)
            .OrderBy(s => s.StepOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestApprovalStep>> GetByApproverIdAsync(Guid approverId)
    {
        return await _context.RequestApprovalSteps
            .Include(s => s.Request)
            .Include(s => s.Approver)
            .Include(s => s.AssignedToUser)
            .Include(s => s.StepTemplate)
            .Where(s => s.ApproverId == approverId || s.AssignedToUserId == approverId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestApprovalStep>> GetPendingByApproverIdAsync(Guid approverId)
    {
        return await _context.RequestApprovalSteps
            .Include(s => s.Request)
            .Include(s => s.Approver)
            .Include(s => s.AssignedToUser)
            .Include(s => s.StepTemplate)
            .Where(s => (s.ApproverId == approverId || s.AssignedToUserId == approverId) &&
                       (s.Status == ApprovalStepStatus.InReview || s.Status == ApprovalStepStatus.Pending))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RequestApprovalStep>> GetByParallelGroupIdAsync(string parallelGroupId, Guid requestId)
    {
        return await _context.RequestApprovalSteps
            .Include(s => s.Request)
            .Include(s => s.Approver)
            .Include(s => s.AssignedToUser)
            .Include(s => s.StepTemplate)
            .Where(s => s.RequestId == requestId &&
                       s.StepTemplate != null &&
                       s.StepTemplate.ParallelGroupId == parallelGroupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(RequestApprovalStep step)
    {
        await _context.RequestApprovalSteps.AddAsync(step);
    }

    public async Task UpdateAsync(RequestApprovalStep step)
    {
        _context.RequestApprovalSteps.Update(step);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var step = await _context.RequestApprovalSteps.FindAsync(id);
        if (step != null)
        {
            _context.RequestApprovalSteps.Remove(step);
        }
    }
}