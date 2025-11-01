using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

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
