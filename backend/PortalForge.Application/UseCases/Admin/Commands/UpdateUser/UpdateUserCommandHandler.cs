using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateUserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user: {UserId} by admin: {AdminId}", request.UserId, request.UpdatedBy);

        // Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Store old values for audit
        var oldValues = new
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            DepartmentId = user.DepartmentId,
            Position = user.Position,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        };

        // Parse role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new ValidationException("Invalid role", new List<string> { $"Role '{request.Role}' is not valid" });
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Department = request.Department;
        user.DepartmentId = request.DepartmentId;
        user.Position = request.Position;
        user.PhoneNumber = request.PhoneNumber;
        user.Role = userRole;
        user.IsActive = request.IsActive;

        if (request.DepartmentId.HasValue)
        {
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value);
            if (department != null)
            {
                user.Department = department.Name;
            }
        }

        await _unitOfWork.UserRepository.UpdateAsync(user);

        // Update role groups - remove all existing and add new ones
        await _unitOfWork.UserRoleGroupRepository.DeleteByUserIdAsync(user.Id);

        foreach (var roleGroupId in request.RoleGroupIds)
        {
            var roleGroup = await _unitOfWork.RoleGroupRepository.GetByIdAsync(roleGroupId);
            if (roleGroup == null)
            {
                _logger.LogWarning("Role group {RoleGroupId} not found, skipping", roleGroupId);
                continue;
            }

            var userRoleGroup = new UserRoleGroup
            {
                UserId = user.Id,
                RoleGroupId = roleGroupId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = request.UpdatedBy
            };

            await _unitOfWork.UserRoleGroupRepository.CreateAsync(userRoleGroup);
        }

        await _unitOfWork.SaveChangesAsync();

        // Log audit
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UpdatedBy,
            Action = "UpdateUser",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Changes = System.Text.Json.JsonSerializer.Serialize(new
            {
                Old = oldValues,
                New = new
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    DepartmentId = user.DepartmentId,
                    Position = user.Position,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.ToString(),
                    IsActive = user.IsActive,
                    RoleGroups = request.RoleGroupIds
                }
            }),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User updated successfully: {UserId}", user.Id);

        return new UpdateUserResult
        {
            UserId = user.Id,
            Message = "User updated successfully"
        };
    }
}

