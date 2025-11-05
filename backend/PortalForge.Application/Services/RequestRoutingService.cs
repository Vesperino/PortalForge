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
                ? await ResolveBySpecificDepartmentAsync(stepTemplate.SpecificDepartmentId.Value)
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
                var errorMessage = stepTemplate.ApproverType switch
                {
                    ApproverType.DirectSupervisor =>
                        "Your department does not have a head (manager) assigned. Please contact HR to resolve this before submitting requests.",

                    ApproverType.DepartmentDirector =>
                        "Your department does not have a director assigned. Please contact HR to resolve this before submitting requests.",

                    ApproverType.SpecificDepartment =>
                        $"The target department has no head assigned. Please contact HR.",

                    ApproverType.UserGroup =>
                        $"The approver group has no active members. Please contact HR.",

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
                // For specific department, use department's substitute
                if (stepTemplate.SpecificDepartmentId.HasValue)
                {
                    var targetDepartment = await _unitOfWork.DepartmentRepository
                        .GetByIdAsync(stepTemplate.SpecificDepartmentId.Value);
                    substitute = targetDepartment?.HeadOfDepartmentSubstitute;
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

        if (dept.HeadOfDepartment == null)
        {
            _logger.LogWarning("Department {DepartmentId} has no head assigned", dept.Id);
            return null;
        }

        if (dept.HeadOfDepartment.Id == submitter.Id)
        {
            _logger.LogWarning("User {UserId} is the head of their own department - cannot approve own request", submitter.Id);
            return null;
        }

        return dept.HeadOfDepartment;
    }

    /// <summary>
    /// Resolves department director from submitter's department.
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

        if (dept.Director == null)
        {
            _logger.LogWarning("Department {DepartmentId} has no director assigned", dept.Id);
            return null;
        }

        if (dept.Director.Id == submitter.Id)
        {
            _logger.LogWarning("User {UserId} is the director of their own department - cannot approve own request", submitter.Id);
            return null;
        }

        return dept.Director;
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
