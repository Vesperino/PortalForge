using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.UpdateRoleGroup;

public class UpdateRoleGroupCommandHandler : IRequestHandler<UpdateRoleGroupCommand, UpdateRoleGroupResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateRoleGroupCommandHandler> _logger;

    public UpdateRoleGroupCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateRoleGroupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateRoleGroupResult> Handle(UpdateRoleGroupCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating role group: {RoleGroupId}", request.RoleGroupId);

        // Get role group
        var roleGroup = await _unitOfWork.RoleGroupRepository.GetByIdAsync(request.RoleGroupId);
        if (roleGroup == null)
        {
            throw new NotFoundException($"Role group with ID {request.RoleGroupId} not found");
        }

        // Check if it's a system role
        if (roleGroup.IsSystemRole)
        {
            throw new ValidationException("Cannot modify system role", new List<string> { "System roles cannot be modified" });
        }

        // Validate name is unique (if changed)
        if (roleGroup.Name != request.Name)
        {
            var existingRoleGroup = await _unitOfWork.RoleGroupRepository.GetByNameAsync(request.Name);
            if (existingRoleGroup != null)
            {
                throw new ValidationException("Role group name already exists", new List<string> { $"Role group with name '{request.Name}' already exists" });
            }
        }

        // Store old values for audit
        var oldValues = new
        {
            roleGroup.Name,
            roleGroup.Description,
            OldPermissionIds = roleGroup.RoleGroupPermissions.Select(rgp => rgp.PermissionId).ToList()
        };

        // Update role group
        roleGroup.Name = request.Name;
        roleGroup.Description = request.Description;
        roleGroup.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.RoleGroupRepository.UpdateAsync(roleGroup);

        // Update permissions - remove all existing and add new ones
        await _unitOfWork.RoleGroupPermissionRepository.DeleteByRoleGroupIdAsync(request.RoleGroupId);

        foreach (var permissionId in request.PermissionIds)
        {
            var permission = await _unitOfWork.PermissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                _logger.LogWarning("Permission {PermissionId} not found, skipping", permissionId);
                continue;
            }

            var roleGroupPermission = new RoleGroupPermission
            {
                RoleGroupId = roleGroup.Id,
                PermissionId = permissionId
            };

            await _unitOfWork.RoleGroupPermissionRepository.CreateAsync(roleGroupPermission);
        }

        await _unitOfWork.SaveChangesAsync();

        // Log audit
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UpdatedBy,
            Action = "UpdateRoleGroup",
            EntityType = "RoleGroup",
            EntityId = roleGroup.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                roleGroup.Name,
                roleGroup.Description,
                PermissionIds = request.PermissionIds
            }),
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role group updated successfully: {RoleGroupId}", roleGroup.Id);

        return new UpdateRoleGroupResult
        {
            Message = "Role group updated successfully"
        };
    }
}

