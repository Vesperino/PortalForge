# Backend: Organizational Structure System

**Module**: Organizational Structure & Hierarchy
**Status**: 📋 Planned
**Last Updated**: 2025-10-31

---

## Overview

This module implements a complete organizational structure system with:
- Department entity with unlimited hierarchical depth
- Employee-supervisor relationships
- Department visibility permissions
- Intelligent request routing based on hierarchy

---

## Domain Entities

### Department

**File**: `backend/PortalForge.Domain/Entities/Department.cs`

```csharp
namespace PortalForge.Domain.Entities;

/// <summary>
/// Represents a department or organizational unit within the company.
/// Supports unlimited hierarchical depth through self-referencing ParentDepartmentId.
/// </summary>
public class Department
{
    /// <summary>
    /// Unique identifier for the department.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the department (e.g., "IT Department", "Backend Team").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the department's purpose and responsibilities.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // ===== HIERARCHY =====

    /// <summary>
    /// Parent department ID. Null if this is a root department (e.g., Board of Directors).
    /// </summary>
    public Guid? ParentDepartmentId { get; set; }

    /// <summary>
    /// Navigation property to parent department.
    /// </summary>
    public Department? ParentDepartment { get; set; }

    /// <summary>
    /// Child departments (sub-departments).
    /// </summary>
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    // ===== MANAGEMENT =====

    /// <summary>
    /// User ID of the department head/manager.
    /// </summary>
    public Guid? HeadOfDepartmentId { get; set; }

    /// <summary>
    /// Navigation property to department head.
    /// </summary>
    public User? HeadOfDepartment { get; set; }

    // ===== STATUS =====

    /// <summary>
    /// Whether the department is active (soft delete support).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the department was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the department was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ===== NAVIGATION =====

    /// <summary>
    /// Employees assigned to this department.
    /// </summary>
    public ICollection<User> Employees { get; set; } = new List<User>();
}
```

### OrganizationalPermission

**File**: `backend/PortalForge.Domain/Entities/OrganizationalPermission.cs`

```csharp
namespace PortalForge.Domain.Entities;

/// <summary>
/// Defines which departments a user can view.
/// By default, users can only see their own department.
/// Admins can grant additional visibility.
/// </summary>
public class OrganizationalPermission
{
    public Guid Id { get; set; }

    /// <summary>
    /// User this permission applies to.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// If true, user can view all departments in the organization.
    /// </summary>
    public bool CanViewAllDepartments { get; set; } = false;

    /// <summary>
    /// JSON array of Guid[] - specific department IDs the user can view.
    /// Stored as JSONB in PostgreSQL for efficient querying.
    /// Example: ["550e8400-e29b-41d4-a716-446655440000", "...]
    /// </summary>
    public string VisibleDepartmentIds { get; set; } = "[]";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Helper method to parse JSON
    public List<Guid> GetVisibleDepartmentIds()
    {
        return JsonSerializer.Deserialize<List<Guid>>(VisibleDepartmentIds) ?? new List<Guid>();
    }

    public void SetVisibleDepartmentIds(List<Guid> departmentIds)
    {
        VisibleDepartmentIds = JsonSerializer.Serialize(departmentIds);
    }
}
```

---

## EF Core Configuration

### DepartmentConfiguration

**File**: `backend/PortalForge.Infrastructure/Persistence/Configurations/DepartmentConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        // Self-referencing relationship (Parent)
        builder.HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

        // Department Head
        builder.HasOne(d => d.HeadOfDepartment)
            .WithMany()
            .HasForeignKey(d => d.HeadOfDepartmentId)
            .OnDelete(DeleteBehavior.SetNull); // If user deleted, set null

        // Employees
        builder.HasMany(d => d.Employees)
            .WithOne(u => u.Department)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete employees with department

        // Indexes
        builder.HasIndex(d => d.ParentDepartmentId)
            .HasDatabaseName("IX_Departments_ParentDepartmentId");

        builder.HasIndex(d => d.HeadOfDepartmentId)
            .HasDatabaseName("IX_Departments_HeadOfDepartmentId");

        builder.HasIndex(d => d.IsActive)
            .HasDatabaseName("IX_Departments_IsActive");

        builder.HasIndex(d => d.Name)
            .HasDatabaseName("IX_Departments_Name");
    }
}
```

---

## Application Layer

### Commands

#### CreateDepartmentCommand

**File**: `backend/PortalForge.Application/UseCases/Departments/Commands/CreateDepartment/CreateDepartmentCommand.cs`

```csharp
using MediatR;

namespace PortalForge.Application.UseCases.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommand : IRequest<Guid>, ITransactionalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
}
```

**Handler**:
```csharp
public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate parent department exists
        if (request.ParentDepartmentId.HasValue)
        {
            var parent = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.ParentDepartmentId.Value);
            if (parent == null)
                throw new NotFoundException($"Parent department {request.ParentDepartmentId} not found");
        }

        // 2. Validate department head exists
        if (request.HeadOfDepartmentId.HasValue)
        {
            var head = await _unitOfWork.UserRepository.GetByIdAsync(request.HeadOfDepartmentId.Value);
            if (head == null)
                throw new NotFoundException($"User {request.HeadOfDepartmentId} not found");
        }

        // 3. Create department
        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId,
            HeadOfDepartmentId = request.HeadOfDepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var departmentId = await _unitOfWork.DepartmentRepository.CreateAsync(department);

        _logger.LogInformation(
            "Created department {DepartmentName} (ID: {DepartmentId})",
            department.Name, departmentId
        );

        return departmentId;
    }
}
```

#### UpdateDepartmentCommand

```csharp
public class UpdateDepartmentCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid DepartmentId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
    public bool? IsActive { get; set; }
}
```

**Validation**:
- Prevent circular references (department can't be its own parent/ancestor)
- Validate head of department is an active user
- Check for duplicate names at same hierarchy level

---

### Queries

#### GetDepartmentTreeQuery

**File**: `backend/PortalForge.Application/UseCases/Departments/Queries/GetDepartmentTree/GetDepartmentTreeQuery.cs`

```csharp
public class GetDepartmentTreeQuery : IRequest<List<DepartmentTreeDto>>
{
    public bool IncludeInactive { get; set; } = false;
}

public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
    public string? HeadOfDepartmentName { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = new();
}
```

**Handler**:
```csharp
public class GetDepartmentTreeQueryHandler : IRequestHandler<GetDepartmentTreeQuery, List<DepartmentTreeDto>>
{
    public async Task<List<DepartmentTreeDto>> Handle(GetDepartmentTreeQuery request, CancellationToken cancellationToken)
    {
        // 1. Get all departments
        var query = _context.Departments
            .Include(d => d.HeadOfDepartment)
            .Include(d => d.Employees)
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(d => d.IsActive);

        var departments = await query.ToListAsync(cancellationToken);

        // 2. Build tree structure
        var dtoMap = departments.Select(d => new DepartmentTreeDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            ParentDepartmentId = d.ParentDepartmentId,
            HeadOfDepartmentId = d.HeadOfDepartmentId,
            HeadOfDepartmentName = d.HeadOfDepartment?.FullName,
            IsActive = d.IsActive,
            EmployeeCount = d.Employees.Count(e => e.IsActive),
            Children = new()
        }).ToList();

        // 3. Link parents and children
        var dtoDict = dtoMap.ToDictionary(d => d.Id);

        foreach (var dto in dtoMap)
        {
            if (dto.ParentDepartmentId.HasValue && dtoDict.ContainsKey(dto.ParentDepartmentId.Value))
            {
                dtoDict[dto.ParentDepartmentId.Value].Children.Add(dto);
            }
        }

        // 4. Return only root departments (children are nested)
        return dtoMap.Where(d => !d.ParentDepartmentId.HasValue).ToList();
    }
}
```

---

## Services

### RequestRoutingService

**File**: `backend/PortalForge.Application/Services/RequestRoutingService.cs`

```csharp
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Services;

/// <summary>
/// Service responsible for routing approval requests to the correct approver
/// based on organizational hierarchy and approval rules.
/// </summary>
public interface IRequestRoutingService
{
    /// <summary>
    /// Resolves the approver for a given approval step template and request submitter.
    /// </summary>
    /// <returns>The approver, or null if no approver found (triggers auto-approval).</returns>
    Task<User?> ResolveApproverAsync(RequestApprovalStepTemplate stepTemplate, User submitter);

    /// <summary>
    /// Checks if a user has a higher supervisor in the hierarchy.
    /// </summary>
    Task<bool> HasHigherSupervisorAsync(User user);
}

public class RequestRoutingService : IRequestRoutingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestRoutingService> _logger;

    public RequestRoutingService(IUnitOfWork unitOfWork, ILogger<RequestRoutingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<User?> ResolveApproverAsync(
        RequestApprovalStepTemplate stepTemplate,
        User submitter)
    {
        _logger.LogDebug(
            "Resolving approver for step {StepOrder}, type {ApproverType}",
            stepTemplate.StepOrder, stepTemplate.ApproverType
        );

        return stepTemplate.ApproverType switch
        {
            ApproverType.DirectSupervisor => submitter.Supervisor,

            ApproverType.Role => await ResolveByRoleAsync(
                stepTemplate.ApproverRole!.Value,
                submitter
            ),

            ApproverType.SpecificUser => await _unitOfWork.UserRepository
                .GetByIdAsync(stepTemplate.SpecificUserId!.Value),

            ApproverType.SpecificDepartment => await GetDepartmentHeadAsync(
                stepTemplate.SpecificDepartmentId!.Value
            ),

            ApproverType.UserGroup => await GetFirstAvailableFromGroupAsync(
                stepTemplate.ApproverGroupId!.Value
            ),

            ApproverType.Submitter => submitter,

            _ => throw new InvalidOperationException($"Unknown approver type: {stepTemplate.ApproverType}")
        };
    }

    private async Task<User?> ResolveByRoleAsync(DepartmentRole targetRole, User submitter)
    {
        var currentUser = submitter;

        // Walk up the supervisor chain until we find someone with the target role or higher
        while (currentUser.Supervisor != null)
        {
            if (currentUser.Supervisor.DepartmentRole >= targetRole)
            {
                _logger.LogDebug(
                    "Found approver {UserId} with role {Role}",
                    currentUser.Supervisor.Id, currentUser.Supervisor.DepartmentRole
                );
                return currentUser.Supervisor;
            }

            currentUser = currentUser.Supervisor;
        }

        // No supervisor found with target role
        _logger.LogWarning(
            "No approver found with role {TargetRole} for user {UserId}",
            targetRole, submitter.Id
        );
        return null;
    }

    private async Task<User?> GetDepartmentHeadAsync(Guid departmentId)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        if (department == null)
        {
            _logger.LogWarning("Department {DepartmentId} not found", departmentId);
            return null;
        }

        if (!department.HeadOfDepartmentId.HasValue)
        {
            _logger.LogWarning("Department {DepartmentId} has no head assigned", departmentId);
            return null;
        }

        return await _unitOfWork.UserRepository.GetByIdAsync(department.HeadOfDepartmentId.Value);
    }

    private async Task<User?> GetFirstAvailableFromGroupAsync(Guid groupId)
    {
        var users = await _unitOfWork.UserRepository.GetUsersByRoleGroupIdAsync(groupId);

        // TODO: Implement "first available" logic
        // For now, return first active user
        return users.FirstOrDefault(u => u.IsActive);
    }

    public async Task<bool> HasHigherSupervisorAsync(User user)
    {
        // Load supervisor if not already loaded
        if (user.Supervisor == null && user.SupervisorId.HasValue)
        {
            user.Supervisor = await _unitOfWork.UserRepository.GetByIdAsync(user.SupervisorId.Value);
        }

        return user.Supervisor != null;
    }
}
```

---

## API Controller

### DepartmentsController

**File**: `backend/PortalForge.Api/Controllers/DepartmentsController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalForge.Application.UseCases.Departments.Commands.*;
using PortalForge.Application.UseCases.Departments.Queries.*;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get organizational structure as a tree.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DepartmentTreeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentTree(
        [FromQuery] bool includeInactive = false)
    {
        var query = new GetDepartmentTreeQuery { IncludeInactive = includeInactive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get department by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        var query = new GetDepartmentByIdQuery { DepartmentId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new department.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentCommand command)
    {
        var departmentId = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetDepartmentById),
            new { id = departmentId },
            departmentId
        );
    }

    /// <summary>
    /// Update department details.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartment(
        Guid id,
        [FromBody] UpdateDepartmentCommand command)
    {
        command.DepartmentId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Delete department (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var command = new DeleteDepartmentCommand { DepartmentId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Get all employees in a department.
    /// </summary>
    [HttpGet("{id}/employees")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentEmployees(Guid id)
    {
        var query = new GetDepartmentEmployeesQuery { DepartmentId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

## Database Migrations

### Migration 1: AddOrganizationalStructure

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddOrganizationalStructure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create Departments table
        migrationBuilder.CreateTable(
            name: "Departments",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Description = table.Column<string>(maxLength: 2000, nullable: true),
                ParentDepartmentId = table.Column<Guid>(nullable: true),
                HeadOfDepartmentId = table.Column<Guid>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Departments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Departments_Departments_ParentDepartmentId",
                    column: x => x.ParentDepartmentId,
                    principalTable: "Departments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Departments_Users_HeadOfDepartmentId",
                    column: x => x.HeadOfDepartmentId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        // Add DepartmentId to Users
        migrationBuilder.AddColumn<Guid>(
            name: "DepartmentId",
            table: "Users",
            nullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Departments_DepartmentId",
            table: "Users",
            column: "DepartmentId",
            principalTable: "Departments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        // Indexes
        migrationBuilder.CreateIndex(
            name: "IX_Departments_ParentDepartmentId",
            table: "Departments",
            column: "ParentDepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Departments_HeadOfDepartmentId",
            table: "Departments",
            column: "HeadOfDepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Departments_IsActive",
            table: "Departments",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_Users_DepartmentId",
            table: "Users",
            column: "DepartmentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Users_Departments_DepartmentId", table: "Users");
        migrationBuilder.DropColumn(name: "DepartmentId", table: "Users");
        migrationBuilder.DropTable(name: "Departments");
    }
}
```

---

## Testing

### Unit Tests Example

```csharp
public class RequestRoutingServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RequestRoutingService _service;

    [Fact]
    public async Task ResolveByRole_FindsManager_WhenSubmitterHasManagerSupervisor()
    {
        // Arrange
        var employee = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Employee
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.Manager
        };

        employee.Supervisor = manager;

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Role,
            ApproverRole = DepartmentRole.Manager
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, employee);

        // Assert
        approver.Should().NotBeNull();
        approver!.Id.Should().Be(manager.Id);
        approver.DepartmentRole.Should().Be(DepartmentRole.Manager);
    }

    [Fact]
    public async Task ResolveByRole_ReturnsNull_WhenPresidentHasNoSupervisor()
    {
        // Arrange
        var president = new User
        {
            Id = Guid.NewGuid(),
            DepartmentRole = DepartmentRole.President,
            Supervisor = null
        };

        var stepTemplate = new RequestApprovalStepTemplate
        {
            ApproverType = ApproverType.Role,
            ApproverRole = DepartmentRole.President
        };

        // Act
        var approver = await _service.ResolveApproverAsync(stepTemplate, president);

        // Assert
        approver.Should().BeNull(); // Triggers auto-approval
    }
}
```

---

## Performance Considerations

### Database Indexes

Critical indexes for performance:
- `IX_Departments_ParentDepartmentId` - For tree traversal
- `IX_Users_DepartmentId` - For department employees
- `IX_Users_SupervisorId` - For supervisor chain traversal
- `IX_Departments_IsActive` - For filtering active departments

### Caching Strategy

Consider caching:
1. **Department Tree** - Rarely changes, cache for 1 hour
2. **User Supervisor Chain** - Cache per user for 15 minutes
3. **Department Employee Counts** - Cache for 5 minutes

### Query Optimization

```csharp
// BAD: N+1 query problem
foreach (var dept in departments)
{
    var head = await _context.Users.FindAsync(dept.HeadOfDepartmentId);
}

// GOOD: Eager loading
var departments = await _context.Departments
    .Include(d => d.HeadOfDepartment)
    .Include(d => d.Employees)
    .ToListAsync();
```

---

## Security Considerations

1. **Authorization**: Only admins can create/update/delete departments
2. **Validation**: Prevent circular references in hierarchy
3. **Soft Delete**: Use `IsActive` flag instead of hard delete
4. **Audit Log**: Log all department changes
5. **Permissions**: Check user has permission to view department

---

## Future Enhancements

1. **Active Directory Sync**: Auto-import departments from AD
2. **Department Budget**: Track department budgets
3. **Department Goals**: Set and track department OKRs
4. **Move Employees**: Bulk transfer employees between departments
5. **Department Analytics**: Headcount trends, turnover rates
