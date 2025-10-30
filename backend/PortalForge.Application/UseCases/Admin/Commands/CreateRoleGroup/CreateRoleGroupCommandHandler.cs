using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.CreateRoleGroup;

public class CreateRoleGroupCommandHandler : IRequestHandler<CreateRoleGroupCommand, CreateRoleGroupResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateRoleGroupCommandHandler> _logger;

    public CreateRoleGroupCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateRoleGroupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateRoleGroupResult> Handle(CreateRoleGroupCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating role group: {Name}", request.Name);

        // Validate name is unique
        var existingRoleGroup = await _unitOfWork.RoleGroupRepository.GetByNameAsync(request.Name);
        if (existingRoleGroup != null)
        {
            throw new ValidationException("Role group name already exists", new List<string> { $"Role group with name '{request.Name}' already exists" });
        }

        // Create role group
        var roleGroup = new RoleGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RoleGroupRepository.CreateAsync(roleGroup);

        // Assign permissions
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
            UserId = request.CreatedBy,
            Action = "CreateRoleGroup",
            EntityType = "RoleGroup",
            EntityId = roleGroup.Id.ToString(),
            Changes = System.Text.Json.JsonSerializer.Serialize(new
            {
                roleGroup.Name,
                roleGroup.Description,
                PermissionIds = request.PermissionIds
            }),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role group created successfully: {RoleGroupId}", roleGroup.Id);

        return new CreateRoleGroupResult
        {
            RoleGroupId = roleGroup.Id,
            Message = "Role group created successfully"
        };
    }
}

