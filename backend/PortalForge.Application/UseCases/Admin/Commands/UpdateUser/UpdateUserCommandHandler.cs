using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISupabaseAuthService _authService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ISupabaseAuthService authService,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
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
            PositionId = user.PositionId,
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
        // Position handling: prefer PositionId, otherwise auto-create/reuse by name
        user.Position = request.Position;
        user.PositionId = request.PositionId;

        if ((request.PositionId == null || request.PositionId == Guid.Empty) && !string.IsNullOrWhiteSpace(request.Position))
        {
            // Try to find an existing position by name (case-insensitive)
            var existingPosition = await _unitOfWork.PositionRepository.GetByNameAsync(request.Position);
            if (existingPosition == null)
            {
                var newPosition = new Position
                {
                    Id = Guid.NewGuid(),
                    Name = request.Position.Trim(),
                    Description = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.PositionRepository.CreateAsync(newPosition);
                user.PositionId = newPosition.Id;
            }
            else
            {
                // Reuse existing and normalize user.Position to canonical name
                user.PositionId = existingPosition.Id;
                user.Position = existingPosition.Name;
            }
        }
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

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            _logger.LogInformation("Admin updating password for user: {UserId}", user.Id);
            var passwordUpdated = await _authService.AdminUpdatePasswordAsync(user.Id, request.NewPassword);

            if (!passwordUpdated)
            {
                _logger.LogError("Failed to update password for user: {UserId}", user.Id);
                throw new ValidationException("Failed to update password", new List<string> { "Password update failed" });
            }

            _logger.LogInformation("Password updated successfully for user: {UserId}", user.Id);
        }

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
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                DepartmentId = user.DepartmentId,
                Position = user.Position,
                PositionId = user.PositionId,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                RoleGroups = request.RoleGroupIds,
                PasswordChanged = !string.IsNullOrWhiteSpace(request.NewPassword)
            }),
            Timestamp = DateTime.UtcNow
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

