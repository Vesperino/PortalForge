using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentTree;

/// <summary>
/// Handler for getting the department tree structure.
/// </summary>
public class GetDepartmentTreeQueryHandler
    : IRequestHandler<GetDepartmentTreeQuery, List<DepartmentTreeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDepartmentTreeQueryHandler> _logger;

    public GetDepartmentTreeQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDepartmentTreeQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<DepartmentTreeDto>> Handle(
        GetDepartmentTreeQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting department tree (IncludeInactive: {IncludeInactive})", request.IncludeInactive);

        // 1. Get all departments
        var allDepartments = await _unitOfWork.DepartmentRepository.GetAllAsync();

        // 2. Filter by active status if needed
        var departments = request.IncludeInactive
            ? allDepartments.ToList()
            : allDepartments.Where(d => d.IsActive).ToList();

        if (!departments.Any())
        {
            _logger.LogInformation("No departments found");
            return new List<DepartmentTreeDto>();
        }

        // 3. Build dictionary for quick lookup
        var departmentDict = departments.ToDictionary(d => d.Id);

        // 4. Build tree structure - start with root departments (no parent)
        var rootDepartments = departments.Where(d => d.ParentDepartmentId == null).ToList();

        _logger.LogInformation("Found {RootCount} root departments out of {TotalCount} total",
            rootDepartments.Count, departments.Count);

        // 5. Convert to DTOs with children
        var result = rootDepartments.Select(d => BuildDepartmentTree(d, departmentDict, 0)).ToList();

        return result;
    }

    /// <summary>
    /// Recursively builds the department tree DTO with all children.
    /// </summary>
    private DepartmentTreeDto BuildDepartmentTree(
        Department department,
        Dictionary<Guid, Department> allDepartments,
        int level)
    {
        var dto = new DepartmentTreeDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            ParentDepartmentId = department.ParentDepartmentId,
            DepartmentHeadId = department.HeadOfDepartmentId,
            DepartmentHeadName = department.HeadOfDepartment != null
                ? $"{department.HeadOfDepartment.FirstName} {department.HeadOfDepartment.LastName}"
                : null,
            IsActive = department.IsActive,
            Level = level,
            EmployeeCount = department.Employees?.Count ?? 0,
            Children = new List<DepartmentTreeDto>()
        };

        // Find and build children recursively
        var children = allDepartments.Values
            .Where(d => d.ParentDepartmentId == department.Id)
            .ToList();

        foreach (var child in children)
        {
            dto.Children.Add(BuildDepartmentTree(child, allDepartments, level + 1));
        }

        return dto;
    }
}
