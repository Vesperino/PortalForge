using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Admin.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeleteUserResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user: {UserId} by admin: {AdminId}", request.UserId, request.DeletedBy);

        // Get user
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Store user data for audit
        var userData = new
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            Position = user.Position,
            Role = user.Role.ToString()
        };

        // Delete user role groups first
        await _unitOfWork.UserRoleGroupRepository.DeleteByUserIdAsync(user.Id);

        // Delete user
        await _unitOfWork.UserRepository.DeleteAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        // Log audit
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.DeletedBy,
            Action = "DeleteUser",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(userData),
            NewValue = null,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User deleted successfully: {UserId}", user.Id);

        return new DeleteUserResult
        {
            Message = "User deleted successfully"
        };
    }
}

