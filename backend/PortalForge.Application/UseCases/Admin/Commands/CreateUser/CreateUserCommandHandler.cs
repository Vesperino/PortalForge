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

        // Register user with Supabase Auth
        var authResult = await _authService.RegisterAsync(
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
        user.Position = request.Position;
        user.PhoneNumber = request.PhoneNumber;
        user.Role = userRole;
        user.MustChangePassword = request.MustChangePassword;

        await _unitOfWork.UserRepository.UpdateAsync(user);

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

