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
            ApproverType.DirectSupervisor => await ResolveDepartmentHeadAsync(submitter),
            ApproverType.DepartmentDirector => await ResolveDepartmentDirectorAsync(submitter),
            ApproverType.SpecificUser => stepTemplate.SpecificUserId.HasValue
                ? await _unitOfWork.UserRepository.GetByIdAsync(stepTemplate.SpecificUserId.Value)
                : null,
            ApproverType.UserGroup => stepTemplate.ApproverGroupId.HasValue
                ? await ResolveByUserGroupAsync(stepTemplate.ApproverGroupId.Value)
                : null,
            ApproverType.SpecificDepartment => stepTemplate.SpecificDepartmentId.HasValue
                ? await ResolveBySpecificDepartmentAsync(
                    stepTemplate.SpecificDepartmentId.Value,
                    stepTemplate.SpecificDepartmentRoleType)
                : null,
            ApproverType.Submitter => submitter,
            _ => null
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
    public async Task<bool> HasHigherSupervisorAsync(User user)
    {
        var sup = await ResolveDepartmentHeadAsync(user);
        return sup != null;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, List<string> Errors)> ValidateApprovalStructureAsync(
        Guid userId,
        IEnumerable<RequestApprovalStepTemplate> stepTemplates)
    {
        var errors = new List<string>();
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

        if (user == null)
        {
            errors.Add("User not found");
            return (false, errors);
        }

        _logger.LogDebug(
            "Validating approval structure for user {UserId} with {StepCount} steps",
            userId,
            stepTemplates.Count());

        foreach (var stepTemplate in stepTemplates.OrderBy(s => s.StepOrder))
        {
            var approver = await ResolveApproverAsync(stepTemplate, user);

            if (approver == null)
            {
                // For DirectSupervisor and DepartmentDirector, allow auto-approval
                // when the submitter is at the top of the hierarchy (no one to escalate to)
                if (stepTemplate.ApproverType == ApproverType.DirectSupervisor ||
                    stepTemplate.ApproverType == ApproverType.DepartmentDirector)
                {
                    _logger.LogInformation(
                        "No approver found for step {StepOrder} (type: {ApproverType}). " +
                        "User {UserId} is at the top of hierarchy - step will be auto-approved.",
                        stepTemplate.StepOrder,
                        stepTemplate.ApproverType,
                        userId);
                    continue; // Allow this - will be auto-approved
                }

                var errorMessage = stepTemplate.ApproverType switch
                {
                    ApproverType.SpecificDepartment =>
                        $"The target department has no head assigned. Please contact HR.",

                    ApproverType.UserGroup =>
                        $"The approver group has no active members. Please contact HR.",

                    ApproverType.SpecificUser =>
                        $"The specified approver user is not available. Please contact HR.",

                    _ => $"Step {stepTemplate.StepOrder}: Unable to determine approver."
                };

                errors.Add(errorMessage);

                _logger.LogWarning(
                    "Validation failed for user {UserId}, step {StepOrder}: {Error}",
                    userId,
                    stepTemplate.StepOrder,
                    errorMessage);
            }
        }

        var isValid = !errors.Any();

        _logger.LogInformation(
            "Approval structure validation for user {UserId}: {Result} ({ErrorCount} errors)",
            userId,
            isValid ? "VALID" : "INVALID",
            errors.Count);

        return (isValid, errors);
    }

    /// <inheritdoc />
    public async Task<Guid?> GetApproverForStepWithSubstituteAsync(
        RequestApprovalStepTemplate stepTemplate,
        User submitter,
        bool checkAvailability = true)
    {
        // First, resolve the primary approver
        var primaryApprover = await ResolveApproverAsync(stepTemplate, submitter);

        if (primaryApprover == null)
        {
            _logger.LogWarning(
                "No primary approver found for step {StepOrder}",
                stepTemplate.StepOrder);
            return null;
        }

        // If we don't need to check availability, return primary approver
        if (!checkAvailability)
        {
            return primaryApprover.Id;
        }

        // Check if primary approver is currently on vacation
        var isOnVacation = await IsUserOnVacationAsync(primaryApprover.Id);

        if (!isOnVacation)
        {
            _logger.LogDebug(
                "Primary approver {ApproverId} is available",
                primaryApprover.Id);
            return primaryApprover.Id;
        }

        _logger.LogInformation(
            "Primary approver {ApproverId} is on vacation, looking for substitute",
            primaryApprover.Id);

        // Try to find substitute based on approver type
        User? substitute = null;

        switch (stepTemplate.ApproverType)
        {
            case ApproverType.DirectSupervisor:
            case ApproverType.DepartmentDirector:
                // For department-based approvers, no automatic substitute
                // Substitute must be manually assigned in department settings
                substitute = null;
                break;

            case ApproverType.SpecificDepartment:
                // For specific department, use department's substitute based on role type
                if (stepTemplate.SpecificDepartmentId.HasValue)
                {
                    var targetDepartment = await _unitOfWork.DepartmentRepository
                        .GetByIdAsync(stepTemplate.SpecificDepartmentId.Value);
                    substitute = stepTemplate.SpecificDepartmentRoleType == DepartmentRoleType.Head
                        ? targetDepartment?.HeadOfDepartmentSubstitute
                        : targetDepartment?.DirectorSubstitute;
                }
                break;

            case ApproverType.UserGroup:
                // For user group, try to find another member who is not on vacation
                if (stepTemplate.ApproverGroupId.HasValue)
                {
                    var groupUsers = await _unitOfWork.RoleGroupRepository
                        .GetUsersInGroupAsync(stepTemplate.ApproverGroupId.Value);

                    foreach (var groupUser in groupUsers)
                    {
                        if (groupUser.Id != primaryApprover.Id && !await IsUserOnVacationAsync(groupUser.Id))
                        {
                            substitute = groupUser;
                            break;
                        }
                    }
                }
                break;
        }

        if (substitute != null)
        {
            _logger.LogInformation(
                "Found substitute approver {SubstituteId} for primary approver {PrimaryId}",
                substitute.Id,
                primaryApprover.Id);
            return substitute.Id;
        }

        _logger.LogWarning(
            "No substitute found for approver {ApproverId} who is on vacation",
            primaryApprover.Id);

        // Return primary approver ID even if on vacation - the request will be queued for them
        return primaryApprover.Id;
    }

    #region Private Resolution Methods

    /// <summary>
    /// Resolves department head (manager) from submitter's department.
    /// If submitter is the head, escalates to department director or parent department.
    /// Handles cases where user is head of multiple departments in the hierarchy.
    /// </summary>
    private async Task<User?> ResolveDepartmentHeadAsync(User submitter)
    {
        if (!submitter.DepartmentId.HasValue)
        {
            _logger.LogWarning("User {UserId} has no department assigned", submitter.Id);
            return null;
        }

        var dept = await _unitOfWork.DepartmentRepository.GetByIdAsync(submitter.DepartmentId.Value);
        if (dept == null)
        {
            _logger.LogWarning("Department {DepartmentId} not found for user {UserId}", submitter.DepartmentId.Value, submitter.Id);
            return null;
        }

        // If department has a head who is not the submitter, use them
        if (dept.HeadOfDepartment != null && dept.HeadOfDepartment.Id != submitter.Id)
        {
            return dept.HeadOfDepartment;
        }

        // Submitter is the department head (or no head assigned) - need to escalate
        _logger.LogInformation(
            "User {UserId} is the head of department {DepartmentId} or no head assigned - escalating approval",
            submitter.Id, dept.Id);

        // First, try to use the department director (if exists and is not the submitter)
        if (dept.Director != null && dept.Director.Id != submitter.Id)
        {
            _logger.LogInformation(
                "Escalating to department director {DirectorId} for user {UserId}",
                dept.Director.Id, submitter.Id);
            return dept.Director;
        }

        // Traverse up the department hierarchy to find a suitable approver
        // (handles case where user is head of multiple departments)
        var visitedDepartments = new HashSet<Guid> { dept.Id };
        var currentDept = dept;

        while (currentDept.ParentDepartmentId.HasValue)
        {
            if (visitedDepartments.Contains(currentDept.ParentDepartmentId.Value))
            {
                _logger.LogWarning("Circular department hierarchy detected at department {DepartmentId}", currentDept.Id);
                break;
            }

            var parentDept = await _unitOfWork.DepartmentRepository.GetByIdAsync(currentDept.ParentDepartmentId.Value);
            if (parentDept == null) break;

            visitedDepartments.Add(parentDept.Id);

            // Try parent department's head
            if (parentDept.HeadOfDepartment != null && parentDept.HeadOfDepartment.Id != submitter.Id)
            {
                _logger.LogInformation(
                    "Escalating to parent department head {HeadId} for user {UserId}",
                    parentDept.HeadOfDepartment.Id, submitter.Id);
                return parentDept.HeadOfDepartment;
            }

            // Try parent department's director
            if (parentDept.Director != null && parentDept.Director.Id != submitter.Id)
            {
                _logger.LogInformation(
                    "Escalating to parent department director {DirectorId} for user {UserId}",
                    parentDept.Director.Id, submitter.Id);
                return parentDept.Director;
            }

            currentDept = parentDept;
        }

        _logger.LogWarning(
            "No suitable approver found for department head {UserId} in department {DepartmentId} after traversing hierarchy",
            submitter.Id, dept.Id);
        return null;
    }

    /// <summary>
    /// Resolves department director from submitter's department.
    /// If submitter is the director, escalates to parent department.
    /// Handles cases where user is director of multiple departments in the hierarchy.
    /// </summary>
    private async Task<User?> ResolveDepartmentDirectorAsync(User submitter)
    {
        if (!submitter.DepartmentId.HasValue)
        {
            _logger.LogWarning("User {UserId} has no department assigned", submitter.Id);
            return null;
        }

        var dept = await _unitOfWork.DepartmentRepository.GetByIdAsync(submitter.DepartmentId.Value);
        if (dept == null)
        {
            _logger.LogWarning("Department {DepartmentId} not found for user {UserId}", submitter.DepartmentId.Value, submitter.Id);
            return null;
        }

        // If department has a director who is not the submitter, use them
        if (dept.Director != null && dept.Director.Id != submitter.Id)
        {
            return dept.Director;
        }

        // Submitter is the department director (or no director assigned) - need to escalate
        _logger.LogInformation(
            "User {UserId} is the director of department {DepartmentId} or no director assigned - escalating approval",
            submitter.Id, dept.Id);

        // Traverse up the department hierarchy to find a suitable approver
        // (handles case where user is director of multiple departments)
        var visitedDepartments = new HashSet<Guid> { dept.Id };
        var currentDept = dept;

        while (currentDept.ParentDepartmentId.HasValue)
        {
            if (visitedDepartments.Contains(currentDept.ParentDepartmentId.Value))
            {
                _logger.LogWarning("Circular department hierarchy detected at department {DepartmentId}", currentDept.Id);
                break;
            }

            var parentDept = await _unitOfWork.DepartmentRepository.GetByIdAsync(currentDept.ParentDepartmentId.Value);
            if (parentDept == null) break;

            visitedDepartments.Add(parentDept.Id);

            // Try parent department's director first (since we're looking for director-level approval)
            if (parentDept.Director != null && parentDept.Director.Id != submitter.Id)
            {
                _logger.LogInformation(
                    "Escalating to parent department director {DirectorId} for user {UserId}",
                    parentDept.Director.Id, submitter.Id);
                return parentDept.Director;
            }

            // Try parent department's head as fallback
            if (parentDept.HeadOfDepartment != null && parentDept.HeadOfDepartment.Id != submitter.Id)
            {
                _logger.LogInformation(
                    "Escalating to parent department head {HeadId} for user {UserId}",
                    parentDept.HeadOfDepartment.Id, submitter.Id);
                return parentDept.HeadOfDepartment;
            }

            currentDept = parentDept;
        }

        _logger.LogWarning(
            "No suitable approver found for department director {UserId} in department {DepartmentId} after traversing hierarchy",
            submitter.Id, dept.Id);
        return null;
    }

    /// <summary>
    /// Resolves approver from a specific department (head or director based on role type).
    /// </summary>
    private async Task<User?> ResolveBySpecificDepartmentAsync(
        Guid departmentId,
        DepartmentRoleType roleType)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);

        if (department == null)
        {
            _logger.LogError(
                "Department {DepartmentId} not found",
                departmentId);
            return null;
        }

        var approver = roleType == DepartmentRoleType.Head
            ? department.HeadOfDepartment
            : department.Director;

        if (approver == null)
        {
            var roleTypeName = roleType == DepartmentRoleType.Head ? "head" : "director";
            _logger.LogWarning(
                "Department {DepartmentId} ({DepartmentName}) has no {RoleType} assigned",
                departmentId,
                department.Name,
                roleTypeName);
            return null;
        }

        return approver;
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

    /// <summary>
    /// Checks if a user is currently on vacation.
    /// </summary>
    private async Task<bool> IsUserOnVacationAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;

        // Check if user has any active vacation that includes today
        var vacationSchedules = await _unitOfWork.VacationScheduleRepository.GetAllAsync();
        var isOnVacation = vacationSchedules.Any(v =>
            v.UserId == userId &&
            v.StartDate.Date <= today &&
            v.EndDate.Date >= today &&
            v.IsActive);

        if (isOnVacation)
        {
            _logger.LogDebug(
                "User {UserId} is currently on vacation",
                userId);
        }

        return isOnVacation;
    }

    #endregion
}
