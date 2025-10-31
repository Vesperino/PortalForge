using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for resolving approvers in request workflows.
/// Implements intelligent routing based on organizational hierarchy.
/// </summary>
public class RequestRoutingService : IRequestRoutingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestRoutingService> _logger;

    public RequestRoutingService(
        IUnitOfWork unitOfWork,
        ILogger<RequestRoutingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<User?> ResolveApproverAsync(
        RequestApprovalStepTemplate stepTemplate,
        User submitter)
    {
        _logger.LogDebug(
            "Resolving approver for step {StepOrder}, type {ApproverType}, submitter {SubmitterId}",
            stepTemplate.StepOrder,
            stepTemplate.ApproverType,
            submitter.Id);

        User? approver = stepTemplate.ApproverType switch
        {
            ApproverType.DirectSupervisor => ResolveByDirectSupervisor(submitter),
            ApproverType.Role => await ResolveByRoleAsync(stepTemplate.ApproverRole!.Value, submitter),
            ApproverType.SpecificUser => stepTemplate.SpecificUser,
            ApproverType.UserGroup => await ResolveByUserGroupAsync(stepTemplate.ApproverGroupId!.Value),
            ApproverType.SpecificDepartment => await ResolveBySpecificDepartmentAsync(stepTemplate.SpecificDepartmentId!.Value),
            ApproverType.Submitter => submitter,
            _ => throw new InvalidOperationException($"Unknown ApproverType: {stepTemplate.ApproverType}")
        };

        if (approver == null)
        {
            _logger.LogInformation(
                "No approver found for step {StepOrder}. Will trigger auto-approval.",
                stepTemplate.StepOrder);
        }
        else
        {
            _logger.LogDebug(
                "Resolved approver {ApproverId} ({ApproverName}) for step {StepOrder}",
                approver.Id,
                approver.FullName,
                stepTemplate.StepOrder);
        }

        return approver;
    }

    /// <inheritdoc />
    public Task<bool> HasHigherSupervisorAsync(User user)
    {
        return Task.FromResult(user.Supervisor != null);
    }

    #region Private Resolution Methods

    /// <summary>
    /// Resolves approver by walking up the supervisor hierarchy to find a user with the target role.
    /// </summary>
    private async Task<User?> ResolveByRoleAsync(DepartmentRole targetRole, User submitter)
    {
        var currentUser = submitter;

        _logger.LogDebug(
            "Walking supervisor chain to find role {TargetRole} starting from {UserId}",
            targetRole,
            submitter.Id);

        // Walk up supervisor chain
        while (currentUser.Supervisor != null)
        {
            currentUser = currentUser.Supervisor;

            _logger.LogDebug(
                "Checking supervisor {SupervisorId} with role {SupervisorRole}",
                currentUser.Id,
                currentUser.DepartmentRole);

            // Check if this supervisor has the target role or higher
            if (currentUser.DepartmentRole >= targetRole)
            {
                _logger.LogDebug(
                    "Found supervisor {SupervisorId} with sufficient role {Role}",
                    currentUser.Id,
                    currentUser.DepartmentRole);

                return currentUser;
            }
        }

        _logger.LogWarning(
            "No supervisor found with role {TargetRole} for submitter {SubmitterId}",
            targetRole,
            submitter.Id);

        return null; // No supervisor with target role found
    }

    /// <summary>
    /// Resolves approver as the direct supervisor of the submitter.
    /// </summary>
    private User? ResolveByDirectSupervisor(User submitter)
    {
        if (submitter.Supervisor == null)
        {
            _logger.LogWarning(
                "User {UserId} has no direct supervisor",
                submitter.Id);
        }

        return submitter.Supervisor;
    }

    /// <summary>
    /// Resolves approver from a specific department (department head).
    /// </summary>
    private async Task<User?> ResolveBySpecificDepartmentAsync(Guid departmentId)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);

        if (department == null)
        {
            _logger.LogError(
                "Department {DepartmentId} not found",
                departmentId);
            return null;
        }

        if (department.HeadOfDepartment == null)
        {
            _logger.LogWarning(
                "Department {DepartmentId} ({DepartmentName}) has no head assigned",
                departmentId,
                department.Name);
            return null;
        }

        return department.HeadOfDepartment;
    }

    /// <summary>
    /// Resolves approver from a user group (returns first available user).
    /// </summary>
    private async Task<User?> ResolveByUserGroupAsync(Guid roleGroupId)
    {
        var users = await _unitOfWork.RoleGroupRepository.GetUsersInGroupAsync(roleGroupId);
        var usersList = users.ToList();

        if (!usersList.Any())
        {
            _logger.LogWarning(
                "RoleGroup {RoleGroupId} has no active users",
                roleGroupId);
            return null;
        }

        // Return first user in group
        // TODO: In future, could implement round-robin or load balancing
        var firstUser = usersList.First();

        _logger.LogDebug(
            "Selected user {UserId} from group {RoleGroupId}",
            firstUser.Id,
            roleGroupId);

        return firstUser;
    }

    #endregion
}
