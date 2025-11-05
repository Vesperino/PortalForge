using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Enhanced service responsible for resolving approvers in request workflows.
/// Extends RequestRoutingService with parallel approval, escalation, and delegation capabilities.
/// </summary>
public class EnhancedRequestRoutingService : RequestRoutingService, IEnhancedRequestRoutingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnhancedRequestRoutingService> _logger;

    public EnhancedRequestRoutingService(
        IUnitOfWork unitOfWork,
        ILogger<EnhancedRequestRoutingService> logger)
        : base(unitOfWork, logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<User>> ResolveParallelApproversAsync(RequestApprovalStepTemplate stepTemplate, User submitter)
    {
        _logger.LogDebug(
            "Resolving parallel approvers for step {StepOrder}, parallel group {ParallelGroupId}",
            stepTemplate.StepOrder,
            stepTemplate.ParallelGroupId);

        var approvers = new List<User>();

        // For parallel steps, we need to resolve all possible approvers based on the type
        switch (stepTemplate.ApproverType)
        {
            case ApproverType.UserGroup:
                if (stepTemplate.ApproverGroupId.HasValue)
                {
                    var groupUsers = await _unitOfWork.RoleGroupRepository
                        .GetUsersInGroupAsync(stepTemplate.ApproverGroupId.Value);
                    approvers.AddRange(groupUsers);
                }
                break;

            case ApproverType.SpecificDepartment:
                if (stepTemplate.SpecificDepartmentId.HasValue)
                {
                    var department = await _unitOfWork.DepartmentRepository
                        .GetByIdAsync(stepTemplate.SpecificDepartmentId.Value);
                    if (department?.HeadOfDepartment != null)
                    {
                        approvers.Add(department.HeadOfDepartment);
                        // Also add substitute if available
                        if (department.HeadOfDepartmentSubstitute != null)
                        {
                            approvers.Add(department.HeadOfDepartmentSubstitute);
                        }
                    }
                }
                break;

            case ApproverType.Role:
                // For role-based parallel approval, find all users with the required role in the hierarchy
                var roleApprovers = await GetAllApproversWithRoleAsync(stepTemplate.ApproverRole, submitter);
                approvers.AddRange(roleApprovers);
                break;

            default:
                // For other types, resolve single approver and add to list
                var singleApprover = await ResolveApproverAsync(stepTemplate, submitter);
                if (singleApprover != null)
                {
                    approvers.Add(singleApprover);
                }
                break;
        }

        // Remove duplicates and the submitter (can't approve own request)
        approvers = approvers
            .Where(a => a.Id != submitter.Id)
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .ToList();

        _logger.LogInformation(
            "Resolved {ApproverCount} parallel approvers for step {StepOrder}",
            approvers.Count,
            stepTemplate.StepOrder);

        return approvers;
    }

    /// <inheritdoc />
    public Task<bool> ShouldEscalateAsync(RequestApprovalStep step)
    {
        if (step.StepTemplate?.EscalationTimeout == null || step.StepTemplate.EscalationUserId == null)
        {
            return Task.FromResult(false);
        }

        var timeElapsed = DateTime.UtcNow - step.CreatedAt;
        var shouldEscalate = timeElapsed >= step.StepTemplate.EscalationTimeout;

        if (shouldEscalate)
        {
            _logger.LogInformation(
                "Step {StepId} should be escalated. Time elapsed: {TimeElapsed}, Timeout: {Timeout}",
                step.Id,
                timeElapsed,
                step.StepTemplate.EscalationTimeout);
        }

        return Task.FromResult(shouldEscalate);
    }

    /// <inheritdoc />
    public async Task<RequestApprovalStep> EscalateRequestStepAsync(Guid stepId)
    {
        var step = await _unitOfWork.RequestApprovalStepRepository.GetByIdAsync(stepId);
        if (step == null)
        {
            throw new InvalidOperationException($"Approval step {stepId} not found");
        }

        if (step.StepTemplate?.EscalationUserId == null)
        {
            throw new InvalidOperationException($"No escalation user defined for step {stepId}");
        }

        var escalationUser = await _unitOfWork.UserRepository.GetByIdAsync(step.StepTemplate.EscalationUserId.Value);
        if (escalationUser == null)
        {
            throw new InvalidOperationException($"Escalation user {step.StepTemplate.EscalationUserId} not found");
        }

        // Update the step to point to the escalation user
        step.AssignedToUserId = escalationUser.Id;
        step.AssignedToUser = escalationUser;
        step.EscalatedAt = DateTime.UtcNow;

        await _unitOfWork.RequestApprovalStepRepository.UpdateAsync(step);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Escalated step {StepId} to user {EscalationUserId}",
            stepId,
            escalationUser.Id);

        return step;
    }

    /// <inheritdoc />
    public async Task<List<User>> GetDelegatedApproversAsync(Guid originalApproverId)
    {
        var currentDate = DateTime.UtcNow;
        
        var delegations = await _unitOfWork.ApprovalDelegationRepository.GetAllAsync();
        var activeDelegations = delegations.Where(d =>
            d.FromUserId == originalApproverId &&
            d.IsActive &&
            d.StartDate <= currentDate &&
            (d.EndDate == null || d.EndDate >= currentDate))
            .ToList();

        var delegatedUsers = new List<User>();
        foreach (var delegation in activeDelegations)
        {
            var delegatedUser = await _unitOfWork.UserRepository.GetByIdAsync(delegation.ToUserId);
            if (delegatedUser != null)
            {
                delegatedUsers.Add(delegatedUser);
            }
        }

        _logger.LogDebug(
            "Found {DelegationCount} active delegations for user {UserId}",
            delegatedUsers.Count,
            originalApproverId);

        return delegatedUsers;
    }

    /// <inheritdoc />
    public async Task<ApprovalDelegation> SetApprovalDelegationAsync(Guid fromUserId, Guid toUserId, DateTime? until, string? reason = null)
    {
        var fromUser = await _unitOfWork.UserRepository.GetByIdAsync(fromUserId);
        var toUser = await _unitOfWork.UserRepository.GetByIdAsync(toUserId);

        if (fromUser == null)
        {
            throw new InvalidOperationException($"User {fromUserId} not found");
        }

        if (toUser == null)
        {
            throw new InvalidOperationException($"User {toUserId} not found");
        }

        var delegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            StartDate = DateTime.UtcNow,
            EndDate = until,
            IsActive = true,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ApprovalDelegationRepository.AddAsync(delegation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Created approval delegation from {FromUserId} to {ToUserId} until {Until}",
            fromUserId,
            toUserId,
            until?.ToString() ?? "indefinite");

        return delegation;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveApprovalDelegationAsync(Guid delegationId)
    {
        var delegation = await _unitOfWork.ApprovalDelegationRepository.GetByIdAsync(delegationId);
        if (delegation == null)
        {
            return false;
        }

        delegation.IsActive = false;
        await _unitOfWork.ApprovalDelegationRepository.UpdateAsync(delegation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Removed approval delegation {DelegationId}",
            delegationId);

        return true;
    }

    /// <inheritdoc />
    public async Task<(List<ApprovalDelegation> DelegationsFrom, List<ApprovalDelegation> DelegationsTo)> GetUserDelegationsAsync(Guid userId)
    {
        var currentDate = DateTime.UtcNow;
        var allDelegations = await _unitOfWork.ApprovalDelegationRepository.GetAllAsync();

        var delegationsFrom = allDelegations.Where(d =>
            d.FromUserId == userId &&
            d.IsActive &&
            d.StartDate <= currentDate &&
            (d.EndDate == null || d.EndDate >= currentDate))
            .ToList();

        var delegationsTo = allDelegations.Where(d =>
            d.ToUserId == userId &&
            d.IsActive &&
            d.StartDate <= currentDate &&
            (d.EndDate == null || d.EndDate >= currentDate))
            .ToList();

        return (delegationsFrom, delegationsTo);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateParallelApprovalRequirementsAsync(string parallelGroupId, Guid requestId)
    {
        var approvalSteps = await _unitOfWork.RequestApprovalStepRepository.GetAllAsync();
        var parallelSteps = approvalSteps.Where(s =>
            s.RequestId == requestId &&
            s.StepTemplate != null &&
            s.StepTemplate.ParallelGroupId == parallelGroupId)
            .ToList();

        if (!parallelSteps.Any())
        {
            return false;
        }

        var approvedSteps = parallelSteps.Count(s => s.Status == ApprovalStepStatus.Approved);
        var minimumRequired = parallelSteps.First().StepTemplate?.MinimumApprovals ?? 1;

        var requirementsMet = approvedSteps >= minimumRequired;

        _logger.LogDebug(
            "Parallel group {ParallelGroupId} validation: {ApprovedCount}/{RequiredCount} approvals",
            parallelGroupId,
            approvedSteps,
            minimumRequired);

        return requirementsMet;
    }

    /// <inheritdoc />
    public async Task<User?> GetEffectiveApproverAsync(RequestApprovalStepTemplate stepTemplate, User submitter, bool considerDelegation = true)
    {
        // First resolve the primary approver
        var primaryApprover = await ResolveApproverAsync(stepTemplate, submitter);
        if (primaryApprover == null)
        {
            return null;
        }

        // If we don't need to consider delegation, return primary approver
        if (!considerDelegation)
        {
            return primaryApprover;
        }

        // Check for active delegations
        var delegatedApprovers = await GetDelegatedApproversAsync(primaryApprover.Id);
        if (delegatedApprovers.Any())
        {
            // Return the first active delegate
            var effectiveApprover = delegatedApprovers.First();
            
            _logger.LogDebug(
                "Using delegated approver {DelegateId} instead of primary approver {PrimaryId}",
                effectiveApprover.Id,
                primaryApprover.Id);

            return effectiveApprover;
        }

        return primaryApprover;
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets all users with a specific role in the organizational hierarchy.
    /// </summary>
    private async Task<List<User>> GetAllApproversWithRoleAsync(DepartmentRole? targetRole, User submitter)
    {
        if (!targetRole.HasValue)
        {
            return new List<User>();
        }

        var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
        var approvers = allUsers.Where(u =>
            u.DepartmentRole >= targetRole.Value &&
            u.Id != submitter.Id &&
            IsInApprovalChain(u, submitter))
            .ToList();

        return approvers;
    }

    /// <summary>
    /// Checks if a user is in the approval chain for the submitter.
    /// </summary>
    private bool IsInApprovalChain(User potentialApprover, User submitter)
    {
        // For now, simple check - same department or parent department
        if (potentialApprover.DepartmentId == submitter.DepartmentId)
        {
            return true;
        }

        // TODO: Implement more sophisticated hierarchy checking
        return false;
    }

    #endregion
}