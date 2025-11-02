using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Permissions.Commands.UpdateOrganizationalPermission;

/// <summary>
/// Handler for updating organizational permission
/// </summary>
public class UpdateOrganizationalPermissionCommandHandler
    : IRequestHandler<UpdateOrganizationalPermissionCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;

    public UpdateOrganizationalPermissionCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
    }

    /// <summary>
    /// Handles the update organizational permission command
    /// </summary>
    public async Task<Unit> Handle(
        UpdateOrganizationalPermissionCommand request,
        CancellationToken cancellationToken)
    {
        // Validate command
        await _validatorService.ValidateAsync(request);

        // Check if permission already exists
        var existingPermission = await _unitOfWork.OrganizationalPermissionRepository
            .GetByUserIdAsync(request.UserId);

        if (existingPermission != null)
        {
            // Update existing permission
            existingPermission.CanViewAllDepartments = request.CanViewAllDepartments;
            existingPermission.SetVisibleDepartmentIds(request.VisibleDepartmentIds);
            existingPermission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.OrganizationalPermissionRepository.UpdateAsync(existingPermission);
        }
        else
        {
            // Create new permission
            var newPermission = new OrganizationalPermission
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CanViewAllDepartments = request.CanViewAllDepartments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            newPermission.SetVisibleDepartmentIds(request.VisibleDepartmentIds);

            await _unitOfWork.OrganizationalPermissionRepository.CreateAsync(newPermission);
        }

        return Unit.Value;
    }
}
