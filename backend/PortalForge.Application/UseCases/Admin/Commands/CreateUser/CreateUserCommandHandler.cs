using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISupabaseAuthService _authService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ISupabaseAuthService authService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user: {Email} by admin: {AdminId}", request.Email, request.CreatedBy);

        // Check if user already exists
        var existingUser = (await _unitOfWork.UserRepository.GetAllAsync())
            .FirstOrDefault(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new ValidationException("User with this email already exists", new List<string> { "Email already exists" });
        }

        // Parse role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new ValidationException("Invalid role", new List<string> { $"Role '{request.Role}' is not valid" });
        }

        // Register user with Supabase Auth via Admin API (no confirmation email)
        var authResult = await _authService.AdminRegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!authResult.Success)
        {
            _logger.LogError("Failed to create user in Supabase Auth: {Error}", authResult.ErrorMessage);
            throw new ValidationException("Failed to create user", new List<string> { authResult.ErrorMessage ?? "Unknown error" });
        }

        // Get the created user from database (created by SupabaseAuthService)
        var user = (await _unitOfWork.UserRepository.GetAllAsync())
            .FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
        {
            _logger.LogError("User was created in Supabase but not found in database");
            throw new ValidationException("User creation failed", new List<string> { "User not found after creation" });
        }

        // Update user with additional information
        user.Department = request.Department;
        user.DepartmentId = request.DepartmentId;
        user.Position = request.Position;
        user.PhoneNumber = request.PhoneNumber;
        user.ProfilePhotoUrl = request.ProfilePhotoUrl;
        user.Role = userRole;
        user.MustChangePassword = request.MustChangePassword;

        // If a position name is provided, ensure a Position record exists and link it
        if (!string.IsNullOrWhiteSpace(request.Position))
        {
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
                user.PositionId = existingPosition.Id;
                user.Position = existingPosition.Name;
            }
        }

        await _unitOfWork.UserRepository.UpdateAsync(user);

        // Auto-create organizational permission if user has a department
        if (request.DepartmentId.HasValue)
        {
            var existingPermission = await _unitOfWork.OrganizationalPermissionRepository.GetByUserIdAsync(user.Id);

            if (existingPermission == null)
            {
                // Create new permission with access to user's department
                var permission = new OrganizationalPermission
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CanViewAllDepartments = false,
                    CreatedAt = DateTime.UtcNow
                };
                permission.SetVisibleDepartmentIds(new List<Guid> { request.DepartmentId.Value });

                await _unitOfWork.OrganizationalPermissionRepository.CreateAsync(permission);
                _logger.LogInformation("Auto-created organizational permission for user {UserId} to view department {DepartmentId}",
                    user.Id, request.DepartmentId.Value);
            }
            else
            {
                // Update existing permission to include new department
                var visibleDepts = existingPermission.GetVisibleDepartmentIds();
                if (!visibleDepts.Contains(request.DepartmentId.Value))
                {
                    visibleDepts.Add(request.DepartmentId.Value);
                    existingPermission.SetVisibleDepartmentIds(visibleDepts);
                    existingPermission.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.OrganizationalPermissionRepository.UpdateAsync(existingPermission);
                    _logger.LogInformation("Updated organizational permission for user {UserId} to include department {DepartmentId}",
                        user.Id, request.DepartmentId.Value);
                }
            }
        }

        // Assign role groups
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
                AssignedBy = request.CreatedBy
            };

            await _unitOfWork.UserRoleGroupRepository.CreateAsync(userRoleGroup);
        }

        await _unitOfWork.SaveChangesAsync();

        // Log audit
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.CreatedBy,
            Action = "CreateUser",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Changes = System.Text.Json.JsonSerializer.Serialize(new
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                Position = user.Position,
                Role = user.Role.ToString(),
                RoleGroups = request.RoleGroupIds
            }),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User created successfully: {UserId}", user.Id);

        return new CreateUserResult
        {
            UserId = user.Id,
            Email = user.Email,
            Message = "User created successfully"
        };
    }
}

