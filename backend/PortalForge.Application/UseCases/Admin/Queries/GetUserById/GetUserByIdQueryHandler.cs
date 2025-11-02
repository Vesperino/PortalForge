using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.Admin.Queries.GetUsers;

namespace PortalForge.Application.UseCases.Admin.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, AdminUserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AdminUserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", request.UserId);

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Get role groups
        var userRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByUserIdAsync(user.Id);
        var roleGroupIds = userRoleGroups.Select(urg => urg.RoleGroupId).ToList();
        var roleGroups = (await _unitOfWork.RoleGroupRepository.GetAllAsync())
            .Where(rg => roleGroupIds.Contains(rg.Id))
            .Select(rg => rg.Name)
            .ToList();

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            DepartmentId = user.DepartmentId,
            Position = user.Position,
            PositionId = user.PositionId,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            MustChangePassword = user.MustChangePassword,
            PhoneNumber = user.PhoneNumber,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            RoleGroups = roleGroups
        };
    }
}

