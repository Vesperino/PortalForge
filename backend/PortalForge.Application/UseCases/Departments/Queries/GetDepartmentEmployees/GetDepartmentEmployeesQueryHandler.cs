using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentEmployees;

/// <summary>
/// Handler for getting all employees in a department.
/// </summary>
public class GetDepartmentEmployeesQueryHandler : IRequestHandler<GetDepartmentEmployeesQuery, List<EmployeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDepartmentEmployeesQueryHandler> _logger;

    public GetDepartmentEmployeesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDepartmentEmployeesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<EmployeeDto>> Handle(
        GetDepartmentEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employees for department: {DepartmentId}", request.DepartmentId);

        // 1. Get department with employees
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.DepartmentId);
            throw new NotFoundException($"Department with ID {request.DepartmentId} not found");
        }

        // 2. Filter employees by active status if needed
        var employees = request.IncludeInactive
            ? department.Employees
            : department.Employees.Where(e => e.IsActive).ToList();

        // 3. Map to DTOs
        var employeeDtos = employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Position = e.Position,
            ProfilePhotoUrl = e.ProfilePhotoUrl,
            DepartmentId = department.Id, // Use department ID from parent
            DepartmentName = department.Name,
            IsActive = e.IsActive
        }).ToList();

        _logger.LogInformation("Found {Count} employees in department {DepartmentName}",
            employeeDtos.Count, department.Name);

        return employeeDtos;
    }
}
