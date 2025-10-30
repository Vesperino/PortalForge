using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.DeleteRoleGroup;

public class DeleteRoleGroupCommandHandler : IRequestHandler<DeleteRoleGroupCommand, DeleteRoleGroupResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteRoleGroupCommandHandler> _logger;

    public DeleteRoleGroupCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteRoleGroupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeleteRoleGroupResult> Handle(DeleteRoleGroupCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting role group: {RoleGroupId}", request.RoleGroupId);

        // Get role group
        var roleGroup = await _unitOfWork.RoleGroupRepository.GetByIdAsync(request.RoleGroupId);
        if (roleGroup == null)
        {
            throw new NotFoundException($"Role group with ID {request.RoleGroupId} not found");
        }

        // Check if it's a system role
        if (roleGroup.IsSystemRole)
        {
            throw new ValidationException("Cannot delete system role", new List<string> { "System roles cannot be deleted" });
        }

        // Check if role group has users
        var userRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByRoleGroupIdAsync(request.RoleGroupId);
        if (userRoleGroups.Any())
        {
            throw new ValidationException("Cannot delete role group with assigned users", new List<string> { $"Role group has {userRoleGroups.Count()} users assigned. Please remove all users before deleting." });
        }

        // Store data for audit
        var roleGroupData = new
        {
            roleGroup.Name,
            roleGroup.Description,
            PermissionIds = roleGroup.RoleGroupPermissions.Select(rgp => rgp.PermissionId).ToList()
        };

        // Delete role group permissions first
        await _unitOfWork.RoleGroupPermissionRepository.DeleteByRoleGroupIdAsync(request.RoleGroupId);

        // Delete role group
        await _unitOfWork.RoleGroupRepository.DeleteAsync(request.RoleGroupId);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.DeletedBy,
            Action = "DeleteRoleGroup",
            EntityType = "RoleGroup",
            EntityId = roleGroup.Id.ToString(),
            Changes = System.Text.Json.JsonSerializer.Serialize(roleGroupData),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role group deleted successfully: {RoleGroupId}", roleGroup.Id);

        return new DeleteRoleGroupResult
        {
            Message = "Role group deleted successfully"
        };
    }
}

