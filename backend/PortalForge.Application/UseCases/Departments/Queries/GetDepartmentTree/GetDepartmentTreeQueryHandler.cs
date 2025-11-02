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
        _logger.LogInformation("Getting department tree (IncludeInactive: {IncludeInactive}, UserId: {UserId})",
            request.IncludeInactive, request.UserId);

        // 1. Get all departments
        var allDepartments = await _unitOfWork.DepartmentRepository.GetAllAsync();

        // 2. Filter by active status if needed
        var departments = request.IncludeInactive
            ? allDepartments.ToList()
            : allDepartments.Where(d => d.IsActive).ToList();

        // 3. Filter by organizational permissions if UserId is provided
        if (request.UserId.HasValue)
        {
            var permission = await _unitOfWork.OrganizationalPermissionRepository.GetByUserIdAsync(request.UserId.Value);

            if (permission != null && !permission.CanViewAllDepartments)
            {
                // User can only see specific departments
                var visibleDepartmentIds = permission.GetVisibleDepartmentIds();

                _logger.LogInformation("User {UserId} can view {Count} specific departments",
                    request.UserId.Value, visibleDepartmentIds.Count);

                // Filter departments to only those the user can see (including parents for tree structure)
                var visibleWithParents = new HashSet<Guid>(visibleDepartmentIds);

                // Add all parent departments to maintain tree structure
                foreach (var deptId in visibleDepartmentIds)
                {
                    var dept = departments.FirstOrDefault(d => d.Id == deptId);
                    if (dept != null)
                    {
                        AddParentDepartments(dept, departments, visibleWithParents);
                    }
                }

                departments = departments.Where(d => visibleWithParents.Contains(d.Id)).ToList();

                _logger.LogInformation("After permission filtering: {Count} departments visible", departments.Count);
            }
            else if (permission != null && permission.CanViewAllDepartments)
            {
                _logger.LogInformation("User {UserId} can view all departments (CanViewAllDepartments = true)", request.UserId.Value);
                // No filtering needed - user sees all departments
            }
            else
            {
                // permission == null - User has no OrganizationalPermission record
                // Show all departments by default (backward compatible behavior)
                _logger.LogInformation("User {UserId} has no organizational permission - showing all departments (default behavior)", request.UserId.Value);
                // No filtering needed
            }
        }

        if (!departments.Any())
        {
            _logger.LogInformation("No departments found");
            return new List<DepartmentTreeDto>();
        }

        // 3. Build dictionary for quick lookup
        var departmentDict = departments.ToDictionary(d => d.Id);

        // 4. Build tree structure - start with root departments (no parent OR parent not in filtered list)
        var rootDepartments = departments.Where(d =>
            d.ParentDepartmentId == null ||
            !departmentDict.ContainsKey(d.ParentDepartmentId.Value)).ToList();

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
            Children = new List<DepartmentTreeDto>(),
            Employees = department.Employees?.Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                Email = e.Email,
                ProfilePhotoUrl = e.ProfilePhotoUrl,
                IsActive = e.IsActive,
                DepartmentId = e.DepartmentId ?? Guid.Empty,
                DepartmentName = department.Name
            }).ToList() ?? new List<EmployeeDto>()
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

    /// <summary>
    /// Recursively adds all parent departments to the visible set to maintain tree structure.
    /// </summary>
    private void AddParentDepartments(
        Department department,
        List<Department> allDepartments,
        HashSet<Guid> visibleSet)
    {
        if (department.ParentDepartmentId.HasValue)
        {
            var parent = allDepartments.FirstOrDefault(d => d.Id == department.ParentDepartmentId.Value);
            if (parent != null && !visibleSet.Contains(parent.Id))
            {
                visibleSet.Add(parent.Id);
                AddParentDepartments(parent, allDepartments, visibleSet);
            }
        }
    }
}
