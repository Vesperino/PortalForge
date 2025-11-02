using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentById;

/// <summary>
/// Handler for getting a department by ID.
/// </summary>
public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDepartmentByIdQueryHandler> _logger;

    public GetDepartmentByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDepartmentByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DepartmentDto> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting department by ID: {DepartmentId}", request.DepartmentId);

        // 1. Get department from repository
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.DepartmentId);
            throw new NotFoundException($"Department with ID {request.DepartmentId} not found");
        }

        // 2. Calculate level (depth in hierarchy)
        var level = await CalculateDepartmentLevel(department.Id);

        // 3. Map to DTO
        var dto = new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            ParentDepartmentId = department.ParentDepartmentId,
            ParentDepartmentName = department.ParentDepartment?.Name,
            DepartmentHeadId = department.HeadOfDepartmentId,
            DepartmentHeadName = department.HeadOfDepartment != null
                ? $"{department.HeadOfDepartment.FirstName} {department.HeadOfDepartment.LastName}"
                : null,
            DepartmentDirectorId = department.DirectorId,
            DepartmentDirectorName = department.Director != null
                ? $"{department.Director.FirstName} {department.Director.LastName}"
                : null,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt,
            Level = level,
            EmployeeCount = department.Employees?.Count ?? 0
        };

        _logger.LogInformation("Department retrieved: {DepartmentName} (Level: {Level})",
            department.Name, level);

        return dto;
    }

    /// <summary>
    /// Calculates the hierarchical level of a department by traversing up to the root.
    /// </summary>
    private async Task<int> CalculateDepartmentLevel(Guid departmentId)
    {
        int level = 0;
        var currentDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);

        while (currentDepartment?.ParentDepartmentId != null)
        {
            level++;
            currentDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(currentDepartment.ParentDepartmentId.Value);
        }

        return level;
    }
}
