using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.DTOs;
using PortalForge.Application.UseCases.Departments.Commands.CreateDepartment;
using PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment;
using PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentById;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentEmployees;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentTree;
using PortalForge.Application.UseCases.Departments.Queries.GetDepartmentVacationCalendar;

namespace PortalForge.Api.Controllers;

/// <summary>
/// Controller for managing departments and organizational structure.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IMediator mediator, ILogger<DepartmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all departments as a flat list.
    /// </summary>
    /// <returns>List of all departments</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        var query = new GetDepartmentTreeQuery();

        // Try to get user ID from JWT claims if user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                query.UserId = userId;
                _logger.LogInformation("Fetching departments for user {UserId}", userId);
            }
        }

        var treeResult = await _mediator.Send(query);

        // Flatten the tree structure to a simple list
        var flatList = new List<DepartmentDto>();
        FlattenTree(treeResult, flatList);

        return Ok(flatList);
    }

    private void FlattenTree(List<DepartmentTreeDto> tree, List<DepartmentDto> flatList)
    {
        foreach (var node in tree)
        {
            flatList.Add(new DepartmentDto
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                ParentDepartmentId = node.ParentDepartmentId,
                DepartmentHeadId = node.DepartmentHeadId,
                DepartmentDirectorId = node.DepartmentDirectorId,
                IsActive = node.IsActive,
                EmployeeCount = node.EmployeeCount
            });

            if (node.Children?.Any() == true)
            {
                FlattenTree(node.Children, flatList);
            }
        }
    }

    /// <summary>
    /// Gets the complete department tree hierarchy.
    /// </summary>
    /// <returns>List of root departments with nested children</returns>
    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DepartmentTreeDto>>> GetTree()
    {
        var query = new GetDepartmentTreeQuery();

        // Try to get user ID from JWT claims if user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                query.UserId = userId;
                _logger.LogInformation("Fetching department tree for user {UserId}", userId);
            }
        }

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single department by ID.
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Department details</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id)
    {
        var query = new GetDepartmentByIdQuery { DepartmentId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets all employees in a department.
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="includeInactive">Whether to include inactive employees</param>
    /// <returns>List of employees in the department</returns>
    [HttpGet("{id}/employees")]
    [AllowAnonymous]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees(
        Guid id,
        [FromQuery] bool includeInactive = false)
    {
        var query = new GetDepartmentEmployeesQuery
        {
            DepartmentId = id,
            IncludeInactive = includeInactive
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets vacation calendar for a department.
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="from">Start date for the calendar range</param>
    /// <param name="to">End date for the calendar range</param>
    /// <returns>List of vacation entries in the specified date range</returns>
    [HttpGet("{id}/vacation-calendar")]
    [Authorize]
    public async Task<ActionResult<List<VacationCalendarEntryDto>>> GetVacationCalendar(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = new GetDepartmentVacationCalendarQuery
        {
            DepartmentId = id,
            FromDate = from ?? DateTime.UtcNow.Date,
            ToDate = to ?? DateTime.UtcNow.AddMonths(3).Date
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    /// <param name="dto">Department creation data</param>
    /// <returns>ID of the created department</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Hr")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateDepartmentDto dto)
    {
        var command = new CreateDepartmentCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentDepartmentId = dto.ParentDepartmentId,
            DepartmentHeadId = dto.DepartmentHeadId,
            DepartmentDirectorId = dto.DepartmentDirectorId
        };

        var departmentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = departmentId }, departmentId);
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="dto">Department update data</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Hr")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var command = new UpdateDepartmentCommand
        {
            DepartmentId = id,
            Name = dto.Name,
            Description = dto.Description,
            ParentDepartmentId = dto.ParentDepartmentId,
            DepartmentHeadId = dto.DepartmentHeadId,
            DepartmentDirectorId = dto.DepartmentDirectorId,
            IsActive = dto.IsActive
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes a department (sets IsActive = false).
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Hr")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeleteDepartmentCommand { DepartmentId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}


