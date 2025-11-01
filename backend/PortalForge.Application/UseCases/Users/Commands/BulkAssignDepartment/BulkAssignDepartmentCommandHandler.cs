using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Users.Commands.BulkAssignDepartment;

/// <summary>
/// Handler for bulk assigning employees to a department.
/// </summary>
public class BulkAssignDepartmentCommandHandler : IRequestHandler<BulkAssignDepartmentCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkAssignDepartmentCommandHandler> _logger;

    public BulkAssignDepartmentCommandHandler(
        IUnitOfWork _unitOfWork,
        ILogger<BulkAssignDepartmentCommandHandler> logger)
    {
        this._unitOfWork = _unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(BulkAssignDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bulk assigning {Count} employees to department {DepartmentId}",
            request.EmployeeIds.Count,
            request.DepartmentId);

        // 1. Verify department exists
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);
        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.DepartmentId);
            throw new NotFoundException($"Department with ID {request.DepartmentId} not found");
        }

        // 2. Update each employee
        int updatedCount = 0;
        foreach (var employeeId in request.EmployeeIds)
        {
            var employee = await _unitOfWork.UserRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                employee.DepartmentId = request.DepartmentId;
                await _unitOfWork.UserRepository.UpdateAsync(employee);

                // Auto-update organizational permission to grant access to new department
                var permission = await _unitOfWork.OrganizationalPermissionRepository.GetByUserIdAsync(employeeId);

                if (permission == null)
                {
                    // Create new permission
                    permission = new OrganizationalPermission
                    {
                        Id = Guid.NewGuid(),
                        UserId = employeeId,
                        CanViewAllDepartments = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    permission.SetVisibleDepartmentIds(new List<Guid> { request.DepartmentId });
                    await _unitOfWork.OrganizationalPermissionRepository.CreateAsync(permission);
                }
                else
                {
                    // Update existing permission
                    var visibleDepts = permission.GetVisibleDepartmentIds();
                    if (!visibleDepts.Contains(request.DepartmentId))
                    {
                        visibleDepts.Add(request.DepartmentId);
                        permission.SetVisibleDepartmentIds(visibleDepts);
                        permission.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.OrganizationalPermissionRepository.UpdateAsync(permission);
                    }
                }

                updatedCount++;
            }
            else
            {
                _logger.LogWarning("Employee not found: {EmployeeId}", employeeId);
            }
        }

        _logger.LogInformation(
            "Successfully assigned {UpdatedCount} employees to department {DepartmentName}",
            updatedCount,
            department.Name);

        return updatedCount;
    }
}
