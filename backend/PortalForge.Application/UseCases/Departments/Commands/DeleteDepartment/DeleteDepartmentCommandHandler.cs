using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using System.Linq;

namespace PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment;

/// <summary>
/// Handler for soft deleting a department.
/// </summary>
public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDepartmentCommandHandler> _logger;

    public DeleteDepartmentCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteDepartmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Soft deleting department: {DepartmentId}", request.DepartmentId);

        // 1. Get existing department with related data
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.DepartmentId);
            throw new NotFoundException($"Department with ID {request.DepartmentId} not found");
        }

        // 2. Unassign all employees from this department
        if (department.Employees != null && department.Employees.Any())
        {
            _logger.LogInformation("Unassigning {Count} employees from department {DepartmentId}",
                department.Employees.Count, request.DepartmentId);

            foreach (var employee in department.Employees)
            {
                employee.DepartmentId = null;
                await _unitOfWork.UserRepository.UpdateAsync(employee);
            }

            _logger.LogInformation("Employees unassigned successfully");
        }

        // 3. Promote child departments to root level (set ParentDepartmentId = null)
        if (department.ChildDepartments != null && department.ChildDepartments.Any())
        {
            _logger.LogInformation("Promoting {Count} subdepartments to root level from department {DepartmentId}",
                department.ChildDepartments.Count, request.DepartmentId);

            foreach (var childDepartment in department.ChildDepartments)
            {
                childDepartment.ParentDepartmentId = null;
                await _unitOfWork.DepartmentRepository.UpdateAsync(childDepartment);
            }

            _logger.LogInformation("Subdepartments promoted to root level successfully");
        }

        // 4. Soft delete by setting IsActive to false
        await _unitOfWork.DepartmentRepository.DeleteAsync(request.DepartmentId);

        _logger.LogInformation("Department soft deleted successfully: {DepartmentId} - {DepartmentName}",
            department.Id, department.Name);

        return MediatR.Unit.Value;
    }
}
