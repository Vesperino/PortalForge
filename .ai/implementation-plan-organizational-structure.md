# Implementation Plan: Organizational Structure & Vacation Management

**Project**: PortalForge - Organizational Structure Enhancement
**Timeline**: 18 days (3.5 weeks)
**Start Date**: TBD
**Status**: 🚧 In Progress - Sprint 1
**Progress**: 5/41 tasks complete (12%)

---

## 📖 Quick Links to Documentation

Before starting, familiarize yourself with these documents:

- 📋 **[ADR-005: Decision Document](.ai/decisions/005-organizational-structure-and-vacation-system.md)** - Overall architecture and decisions
- 🔧 **[Backend: Organizational Structure](.ai/backend/organizational-structure.md)** - Department entity, routing service, API
- 📅 **[Backend: Vacation System](.ai/backend/vacation-schedule-system.md)** - VacationSchedule entity, conflict detection, exports
- 🎨 **[Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)** - 3 calendar views, components, composables

---

## 📖 How to Use This Plan

### For Agents/Developers:

1. **Start with Sprint 1, Task 1.1** - Work sequentially through tasks
2. **Check the box** `[x]` when task is complete (change `[ ]` to `[x]`)
3. **Read linked documentation** - Each task has `📖 Reference:` link to detailed docs
4. **Follow acceptance criteria** - Task is done only when all criteria met
5. **Update progress** - Update the progress percentages at bottom of file
6. **Commit frequently** - Commit after each completed task

### Task Status Indicators:

- `[ ]` - Not started
- `[x]` - Complete
- `⭐` - Critical task (blocking other tasks)
- `📖 Reference:` - Link to detailed documentation

### Progress Tracking:

Update these sections as you complete tasks:
- Individual checkboxes `[x]`
- Sprint progress bars (e.g., `█████░░░ 5/8 tasks`)
- Overall progress at top of file

---

## 📋 Executive Summary

This implementation plan details the development of:
1. **Unlimited hierarchical organizational structure** (Department entity)
2. **Intelligent vacation management system** with substitute routing
3. **Team vacation calendar** with 3 views (Timeline, Grid, List)
4. **Automatic request routing** with multi-level approval
5. **Conflict detection and alerts** for vacation coverage

**Total Tasks**: 41 major tasks across 6 sprints

---

## 🎯 Sprint Overview

| Sprint | Duration | Focus | Tasks | Progress |
|--------|----------|-------|-------|----------|
| Sprint 1 | Days 1-4 | Backend Foundation | 8 tasks | █████░░░ 5/8 |
| Sprint 2 | Days 5-7 | Routing & Vacation Logic | 7 tasks | ░░░░░░░ 0/7 |
| Sprint 3 | Days 8-11 | Frontend - Vacation Calendar | 8 tasks | ░░░░░░░░ 0/8 |
| Sprint 4 | Days 12-14 | Frontend - Structure & Requests | 7 tasks | ░░░░░░░ 0/7 |
| Sprint 5 | Days 15-16 | Permissions & Notifications | 5 tasks | ░░░░░ 0/5 |
| Sprint 6 | Days 17-18 | Testing & Documentation | 6 tasks | ░░░░░░ 0/6 |

**Overall Progress**: █░░░░░░░░░ 5/41 (12%)

---

## 📅 SPRINT 1: Backend Foundation (Days 1-4)

**Goal**: Create core entities, enums, and database migrations

**Progress**: █████░░░ 5/8 tasks complete (62.5%)

### Day 1-2: Core Entities

#### [x] Task 1.1: Department Entity ⭐ CRITICAL

**File**: `backend/PortalForge.Domain/Entities/Department.cs`

**📖 Reference**: [Backend: Organizational Structure - Department Entity](.ai/backend/organizational-structure.md#department)

**Requirements**:
- Unlimited hierarchical depth (ParentDepartmentId)
- Department head assignment (HeadOfDepartmentId)
- IsActive flag for soft delete
- Timestamps (CreatedAt, UpdatedAt)
- Navigation properties (Employees, ChildDepartments)

**Deliverable**:
```csharp
public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Hierarchy
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    // Department head
    public Guid? HeadOfDepartmentId { get; set; }
    public User? HeadOfDepartment { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Employees { get; set; } = new List<User>();
}
```

**Acceptance Criteria**:
- ✅ Entity compiles without errors
- ✅ Self-referencing navigation works (ParentDepartment/ChildDepartments)
- ✅ Can create department tree 5+ levels deep
- ✅ XML documentation added to all properties

**Dependencies**: None

**Estimated Time**: 1 hour

---

#### [x] Task 1.2: VacationSchedule Entity ⭐ CRITICAL

**File**: `backend/PortalForge.Domain/Entities/VacationSchedule.cs`

**📖 Reference**: [Backend: Vacation System - VacationSchedule Entity](.ai/backend/vacation-schedule-system.md#vacationschedule)

**Requirements**:
- Link to User (who's on vacation)
- Link to Substitute (who covers)
- Link to source Request
- Status tracking (Scheduled, Active, Completed, Cancelled)
- Computed property: DaysCount

**Deliverable**:
```csharp
public class VacationSchedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public Guid SubstituteUserId { get; set; }
    public User Substitute { get; set; } = null!;

    public Guid SourceRequestId { get; set; }
    public Request SourceRequest { get; set; } = null!;

    public VacationStatus Status { get; set; } = VacationStatus.Scheduled;
    public DateTime CreatedAt { get; set; }

    public int DaysCount => (EndDate.Date - StartDate.Date).Days + 1;
    public bool IsActive => Status == VacationStatus.Active;
}

public enum VacationStatus
{
    Scheduled,
    Active,
    Completed,
    Cancelled
}
```

**Acceptance Criteria**:
- ✅ Can create vacation schedule from approved request
- ✅ Can query by date range (StartDate/EndDate)
- ✅ Can find active vacation for user
- ✅ DaysCount computed correctly (includes start and end date)

**Dependencies**: None

**Estimated Time**: 1 hour

---

#### [x] Task 1.3: OrganizationalPermission Entity

**File**: `backend/PortalForge.Domain/Entities/OrganizationalPermission.cs`

**📖 Reference**: [Backend: Organizational Structure - OrganizationalPermission](.ai/backend/organizational-structure.md#organizationalpermission)

**Requirements**:
- Per-user visibility permissions
- Can view all departments flag
- Specific department IDs (JSON array)
- Helper methods to parse/set JSON

**Deliverable**:
```csharp
public class OrganizationalPermission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool CanViewAllDepartments { get; set; } = false;
    public string VisibleDepartmentIds { get; set; } = "[]"; // JSON

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Helper methods
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

**Acceptance Criteria**:
- ✅ One-to-one relationship with User
- ✅ Default = user sees only their department
- ✅ Admin can grant additional department access
- ✅ Helper methods work correctly

**Dependencies**: None

**Estimated Time**: 30 minutes

---

### Day 3: Enums and Extensions

#### [x] Task 1.4: Extended DepartmentRole Enum

**File**: `backend/PortalForge.Domain/Enums/DepartmentRole.cs`

**📖 Reference**: [ADR-005 - Enhanced DepartmentRole](.ai/decisions/005-organizational-structure-and-vacation-system.md#extended-departmentrole)

**Requirements**:
- Add 4 new roles: TeamLeader, VicePresident, President, BoardMember
- Maintain backward compatibility with existing roles
- Use numeric values for proper comparison (>=)

**Deliverable**:
```csharp
public enum DepartmentRole
{
    Employee = 0,       // Existing
    TeamLeader = 1,     // NEW
    Manager = 2,        // Existing (was 1, now 2)
    Director = 3,       // Existing (was 2, now 3)
    VicePresident = 4,  // NEW
    President = 5,      // NEW
    BoardMember = 6     // NEW
}
```

**Acceptance Criteria**:
- ✅ Existing requests still work (Employee=0, Manager=2, Director=3)
- ✅ Can assign new roles to users
- ✅ Hierarchy comparison works: `user.DepartmentRole >= DepartmentRole.Manager`
- ✅ Database migration handles existing data

**Dependencies**: None

**Estimated Time**: 30 minutes

---

#### [x] Task 1.5: Enhanced ApproverType Enum

**File**: `backend/PortalForge.Domain/Enums/ApproverType.cs`

**📖 Reference**: [ADR-005 - Enhanced ApproverType](.ai/decisions/005-organizational-structure-and-vacation-system.md#enhanced-approvertype)

**Requirements**:
- Add SpecificDepartment (head of specific department)
- Add DirectSupervisor (immediate supervisor)
- Maintain existing types (Role, SpecificUser, UserGroup, Submitter)

**Deliverable**:
```csharp
public enum ApproverType
{
    Role,                  // Existing - hierarchical role (Manager, Director)
    SpecificUser,          // Existing - specific user by ID
    UserGroup,             // Existing - any user from a RoleGroup
    Submitter,             // Existing - the person who submitted
    SpecificDepartment,    // NEW - head of specific department
    DirectSupervisor       // NEW - immediate supervisor
}
```

**Acceptance Criteria**:
- ✅ Can select all 6 types in template creation UI
- ✅ Routing logic handles all types
- ✅ Validation ensures correct fields are set per type

**Dependencies**: None

**Estimated Time**: 15 minutes

---

### Day 4: Database Migrations

#### [ ] Task 1.6: EF Core Configurations

**Files**:
- `backend/PortalForge.Infrastructure/Persistence/Configurations/DepartmentConfiguration.cs`
- `backend/PortalForge.Infrastructure/Persistence/Configurations/VacationScheduleConfiguration.cs`
- `backend/PortalForge.Infrastructure/Persistence/Configurations/OrganizationalPermissionConfiguration.cs`

**📖 Reference**: [Backend: Organizational Structure - EF Core Configuration](.ai/backend/organizational-structure.md#ef-core-configuration)

**Requirements**:
- Fluent API configurations for all 3 entities
- Indexes on frequently queried columns
- Foreign key constraints with proper cascade behavior
- JSON column configuration for VisibleDepartmentIds

**Deliverable Example** (DepartmentConfiguration):
```csharp
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(2000);
        builder.Property(d => d.IsActive).IsRequired().HasDefaultValue(true);

        // Self-referencing (Parent)
        builder.HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department Head
        builder.HasOne(d => d.HeadOfDepartment)
            .WithMany()
            .HasForeignKey(d => d.HeadOfDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(d => d.ParentDepartmentId);
        builder.HasIndex(d => d.HeadOfDepartmentId);
        builder.HasIndex(d => d.IsActive);
        builder.HasIndex(d => d.Name);
    }
}
```

**Acceptance Criteria**:
- ✅ All navigation properties configured
- ✅ Indexes on: ParentDepartmentId, HeadOfDepartmentId, IsActive, Name, StartDate, EndDate
- ✅ Cascade delete rules correct (Restrict for departments, SetNull for heads)
- ✅ JSON column for VisibleDepartmentIds configured as JSONB (PostgreSQL)

**Dependencies**: Tasks 1.1, 1.2, 1.3

**Estimated Time**: 2 hours

---

#### [ ] Task 1.7: Migration - AddOrganizationalStructure

**File**: `backend/PortalForge.Infrastructure/Migrations/YYYYMMDD_AddOrganizationalStructure.cs`

**📖 Reference**: [Backend: Organizational Structure - Database Migrations](.ai/backend/organizational-structure.md#database-migrations)

**Requirements**:
- Create Departments table
- Add User.DepartmentId (nullable initially for migration)
- Create indexes
- Handle self-referencing FK correctly

**Command to run**:
```bash
cd backend/PortalForge.Infrastructure
dotnet ef migrations add AddOrganizationalStructure --project ../PortalForge.Infrastructure --startup-project ../PortalForge.Api
```

**SQL Preview** (for review):
```sql
CREATE TABLE "Departments" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT NULL,
    "ParentDepartmentId" UUID NULL,
    "HeadOfDepartmentId" UUID NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NULL,
    CONSTRAINT "FK_Departments_Departments"
        FOREIGN KEY ("ParentDepartmentId") REFERENCES "Departments"("Id"),
    CONSTRAINT "FK_Departments_Users"
        FOREIGN KEY ("HeadOfDepartmentId") REFERENCES "Users"("Id")
);

ALTER TABLE "Users" ADD COLUMN "DepartmentId" UUID NULL;
ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Departments"
    FOREIGN KEY ("DepartmentId") REFERENCES "Departments"("Id");
```

**Acceptance Criteria**:
- ✅ Migration runs successfully (`dotnet ef database update`)
- ✅ Can rollback migration without errors
- ✅ Existing User data intact (Department string column preserved temporarily)
- ✅ Can create department records via DbContext

**Dependencies**: Task 1.6

**Estimated Time**: 1 hour

---

#### [ ] Task 1.8: Migration - AddVacationScheduleSystem

**File**: `backend/PortalForge.Infrastructure/Migrations/YYYYMMDD_AddVacationScheduleSystem.cs`

**📖 Reference**: [Backend: Vacation System - Database Migrations](.ai/backend/vacation-schedule-system.md)

**Requirements**:
- Create VacationSchedules table
- Create OrganizationalPermissions table
- Extend RequestApprovalStepTemplates (add SpecificDepartmentId)
- Extend RequestTemplates (add RequiresSubstituteSelection)

**Command to run**:
```bash
dotnet ef migrations add AddVacationScheduleSystem --project ../PortalForge.Infrastructure --startup-project ../PortalForge.Api
```

**SQL Preview**:
```sql
CREATE TABLE "VacationSchedules" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "StartDate" DATE NOT NULL,
    "EndDate" DATE NOT NULL,
    "SubstituteUserId" UUID NOT NULL,
    "SourceRequestId" UUID NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL
    -- Foreign keys omitted for brevity
);

CREATE TABLE "OrganizationalPermissions" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL UNIQUE,
    "CanViewAllDepartments" BOOLEAN NOT NULL DEFAULT FALSE,
    "VisibleDepartmentIds" JSONB NOT NULL DEFAULT '[]',
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NULL
);

ALTER TABLE "RequestApprovalStepTemplates"
ADD COLUMN "SpecificDepartmentId" UUID NULL;

ALTER TABLE "RequestTemplates"
ADD COLUMN "RequiresSubstituteSelection" BOOLEAN NOT NULL DEFAULT FALSE;
```

**Acceptance Criteria**:
- ✅ All tables created successfully
- ✅ Foreign keys enforced (CASCADE, SET NULL, RESTRICT as appropriate)
- ✅ Indexes created on: UserId, Status, StartDate, EndDate
- ✅ JSONB column works (can insert/query JSON arrays)

**Dependencies**: Task 1.6

**Estimated Time**: 1.5 hours

---

## 📅 SPRINT 2: Routing & Vacation Logic (Days 5-7)

**Goal**: Implement intelligent request routing and vacation management services

**Progress**: ░░░░░░░ 0/7 tasks complete

### Day 5: Request Routing Service

#### [ ] Task 2.1: RequestRoutingService ⭐ CRITICAL

**File**: `backend/PortalForge.Application/Services/RequestRoutingService.cs`

**📖 Reference**: [Backend: Organizational Structure - RequestRoutingService](.ai/backend/organizational-structure.md#requestroutingservice)

**Requirements**:
- Resolve approver based on ApproverType (all 6 types)
- Walk supervisor hierarchy for Role type
- Support DirectSupervisor, SpecificDepartment, UserGroup
- Return null when no higher supervisor (triggers auto-approval)

**Interface**:
```csharp
public interface IRequestRoutingService
{
    Task<User?> ResolveApproverAsync(RequestApprovalStepTemplate stepTemplate, User submitter);
    Task<bool> HasHigherSupervisorAsync(User user);
}
```

**Key Methods**:
```csharp
private async Task<User?> ResolveByRoleAsync(DepartmentRole targetRole, User submitter)
{
    var currentUser = submitter;

    // Walk up supervisor chain
    while (currentUser.Supervisor != null)
    {
        if (currentUser.Supervisor.DepartmentRole >= targetRole)
        {
            return currentUser.Supervisor;
        }
        currentUser = currentUser.Supervisor;
    }

    return null; // No supervisor with target role found
}
```

**Acceptance Criteria**:
- ✅ Correctly resolves all 6 ApproverType scenarios
- ✅ Returns null for President submitting (no higher supervisor)
- ✅ Handles missing/null supervisors gracefully
- ✅ Logs debug information for troubleshooting
- ✅ Throws appropriate exceptions for invalid configurations

**Dependencies**: Tasks 1.1-1.8 (all backend foundation)

**Estimated Time**: 3 hours

---

#### [ ] Task 2.2: Auto-Approval Logic

**File**: `backend/PortalForge.Application/UseCases/Requests/Commands/SubmitRequest/SubmitRequestCommandHandler.cs`

**📖 Reference**: [ADR-005 - Auto-Approval Logic](.ai/decisions/005-organizational-structure-and-vacation-system.md#auto-approval-logic)

**Requirements**:
- When ResolveApprover returns null → auto-approve step
- Create approval step with status = Approved
- Set comment = "Auto-approved (no higher supervisor)"
- Log audit trail for compliance

**Implementation**:
```csharp
var approver = await _routingService.ResolveApproverAsync(stepTemplate, submitter);

if (approver == null)
{
    // No higher supervisor - auto-approve
    var autoStep = new RequestApprovalStep
    {
        StepOrder = stepTemplate.StepOrder,
        ApproverId = submitter.Id,
        ApproverName = "Auto-approved",
        Status = ApprovalStepStatus.Approved,
        Comment = "Automatically approved - submitter has no higher supervisor",
        StartedAt = DateTime.UtcNow,
        FinishedAt = DateTime.UtcNow
    };
    request.ApprovalSteps.Add(autoStep);

    _logger.LogInformation(
        "Auto-approved step {Order} for request {RequestId} - user {UserId} has no higher supervisor",
        stepTemplate.StepOrder, request.Id, submitter.Id
    );
}
```

**Acceptance Criteria**:
- ✅ President's vacation request auto-approves
- ✅ Audit log shows auto-approval with reason
- ✅ Request status = Approved if all steps auto-approved
- ✅ Email notification sent to submitter about auto-approval

**Dependencies**: Task 2.1

**Estimated Time**: 2 hours

---

### Day 6: Vacation Schedule Service

#### [ ] Task 2.3: VacationScheduleService ⭐ CRITICAL

**File**: `backend/PortalForge.Application/Services/VacationScheduleService.cs`

**📖 Reference**: [Backend: Vacation System - VacationScheduleService](.ai/backend/vacation-schedule-system.md#vacationscheduleservice)

**Requirements**:
- Create vacation schedule from approved request
- Get active substitute for user (if on vacation)
- Get team vacation calendar with statistics
- Detect vacation conflicts (>30% threshold)
- Calculate statistics (days, coverage %)

**Key Methods**:
```csharp
public interface IVacationScheduleService
{
    Task CreateFromApprovedRequestAsync(Request vacationRequest);
    Task<User?> GetActiveSubstituteAsync(Guid userId);
    Task<VacationCalendar> GetTeamCalendarAsync(Guid departmentId, DateTime start, DateTime end);
    Task<List<VacationAlert>> DetectConflicts(List<VacationSchedule> vacations, int teamSize);
    Task UpdateVacationStatusesAsync(); // For daily job
}
```

**Conflict Detection Algorithm**:
```csharp
// For each date in range
foreach (var date in dateRange)
{
    var onVacationCount = vacations.Count(v =>
        v.StartDate <= date && v.EndDate >= date
    );

    var coveragePercent = (double)onVacationCount / teamSize * 100;

    if (coveragePercent >= 30)
    {
        alerts.Add(new VacationAlert
        {
            Type = coveragePercent >= 50 ? CRITICAL : LOW,
            CoveragePercent = coveragePercent,
            Message = $"⚠️ {onVacationCount}/{teamSize} on vacation ({coveragePercent:F0}%)"
        });
    }
}
```

**Acceptance Criteria**:
- ✅ Vacation created when vacation request approved
- ✅ Substitute correctly retrieved during vacation dates
- ✅ Alerts generated when >30% team on vacation
- ✅ Statistics calculated correctly (currentlyOnVacation, scheduled, etc.)
- ✅ Validates substitute is not the user themselves
- ✅ Validates substitute is active user

**Dependencies**: Tasks 1.2, 1.8

**Estimated Time**: 4 hours

---

#### [ ] Task 2.4: Substitute Routing in Approval

**File**: `backend/PortalForge.Application/UseCases/Requests/Commands/ApproveRequestStep/ApproveRequestStepCommandHandler.cs`

**📖 Reference**: [Backend: Vacation System - Integration with Request Approval](.ai/backend/vacation-schedule-system.md#substitute-routing)

**Requirements**:
- Before assigning next approver, check if they're on vacation
- If on vacation, use substitute from VacationSchedule
- Log substitution in approval step comment
- Notify both approver and substitute

**Implementation**:
```csharp
// After determining next approver
var nextApprover = await _routingService.ResolveApproverAsync(nextStepTemplate, submitter);

// Check if approver is on vacation
var substitute = await _vacationService.GetActiveSubstituteAsync(nextApprover.Id);
if (substitute != null)
{
    _logger.LogInformation(
        "Approver {ApproverId} is on vacation, routing to substitute {SubstituteId}",
        nextApprover.Id, substitute.Id
    );

    var nextStep = new RequestApprovalStep
    {
        ApproverId = substitute.Id,
        ApproverName = substitute.FullName,
        Comment = $"Routed to substitute for {nextApprover.FullName} (on vacation)",
        // ...
    };
}
```

**Acceptance Criteria**:
- ✅ Request routed to substitute when approver on vacation
- ✅ Original approver can still see request (but cannot approve)
- ✅ Audit log shows substitution occurred
- ✅ Notification sent to substitute about pending approval

**Dependencies**: Tasks 2.1, 2.3

**Estimated Time**: 2 hours

---

### Day 7: Background Jobs & Endpoints

#### [ ] Task 2.5: Daily Job - Update Vacation Statuses

**File**: `backend/PortalForge.Infrastructure/BackgroundJobs/UpdateVacationStatusesJob.cs`

**📖 Reference**: [Backend: Vacation System - Background Job](.ai/backend/vacation-schedule-system.md#background-job)

**Requirements**:
- Run daily at 00:01 UTC
- Activate vacations where StartDate <= today
- Complete vacations where EndDate < today
- Log status changes
- Send notifications when vacation starts/ends

**Implementation**:
```csharp
public class UpdateVacationStatusesJob : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var tomorrow = now.Date.AddDays(1).AddMinutes(1); // 00:01 tomorrow
            var delay = tomorrow - now;

            await Task.Delay(delay, stoppingToken);

            using (var scope = _serviceProvider.CreateScope())
            {
                var vacationService = scope.ServiceProvider
                    .GetRequiredService<IVacationScheduleService>();

                await vacationService.UpdateVacationStatusesAsync();
            }
        }
    }
}
```

**Registration** in `Program.cs`:
```csharp
builder.Services.AddHostedService<UpdateVacationStatusesJob>();
```

**Acceptance Criteria**:
- ✅ Vacations activated on start date (Scheduled → Active)
- ✅ Vacations completed on end date (Active → Completed)
- ✅ Runs automatically at midnight UTC
- ✅ Handles errors gracefully (logs and continues)
- ✅ Notifications sent when status changes

**Dependencies**: Task 2.3

**Estimated Time**: 2 hours

---

#### [ ] Task 2.6: Department CRUD Endpoints

**File**: `backend/PortalForge.Api/Controllers/DepartmentsController.cs`

**📖 Reference**: [Backend: Organizational Structure - API Controller](.ai/backend/organizational-structure.md#api-controller)

**Endpoints to implement**:
- `GET /api/departments` - Get all (tree structure)
- `GET /api/departments/{id}` - Get one
- `POST /api/departments` - Create (Admin only)
- `PUT /api/departments/{id}` - Update (Admin only)
- `DELETE /api/departments/{id}` - Soft delete (Admin only)
- `GET /api/departments/{id}/employees` - Get employees

**Key Endpoint Example**:
```csharp
[HttpGet]
[ProducesResponseType(typeof(List<DepartmentTreeDto>), 200)]
public async Task<IActionResult> GetDepartmentTree([FromQuery] bool includeInactive = false)
{
    var query = new GetDepartmentTreeQuery { IncludeInactive = includeInactive };
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**Acceptance Criteria**:
- ✅ Can create department tree via API
- ✅ Can move departments (change ParentDepartmentId)
- ✅ Can assign department head
- ✅ Tree structure returned efficiently (no N+1 queries)
- ✅ Only Admin can create/update/delete
- ✅ Swagger documentation generated

**Dependencies**: Tasks 1.1, 1.7

**Estimated Time**: 3 hours

---

#### [ ] Task 2.7: Vacation Schedule Endpoints

**File**: `backend/PortalForge.Api/Controllers/VacationSchedulesController.cs`

**📖 Reference**: [Backend: Vacation System - API Controller](.ai/backend/vacation-schedule-system.md#api-controller)

**Endpoints to implement**:
- `GET /api/vacation-schedules/team?departmentId={id}&month={month}` - Team calendar
- `GET /api/vacation-schedules/my-substitutions` - My active substitutions
- `GET /api/vacation-schedules/export/pdf` - Export to PDF
- `GET /api/vacation-schedules/export/excel` - Export to Excel

**Key Endpoint Example**:
```csharp
[HttpGet("team")]
[Authorize(Policy = "ManagerOrAbove")]
[ProducesResponseType(typeof(VacationCalendar), 200)]
public async Task<IActionResult> GetTeamCalendar(
    [FromQuery] Guid? departmentId,
    [FromQuery] int year,
    [FromQuery] int month)
{
    var deptId = departmentId ?? CurrentUser.DepartmentId;
    var startDate = new DateTime(year, month, 1);
    var endDate = startDate.AddMonths(1).AddDays(-1);

    var calendar = await _vacationService.GetTeamCalendarAsync(deptId, startDate, endDate);
    return Ok(calendar);
}
```

**Acceptance Criteria**:
- ✅ Returns vacations with statistics and alerts
- ✅ PDF export generates correctly (QuestPDF)
- ✅ Excel export generates correctly (EPPlus)
- ✅ Only managers can view team calendar
- ✅ Users can view their own substitutions
- ✅ Export downloads with correct filename

**Dependencies**: Task 2.3

**Estimated Time**: 4 hours (includes PDF/Excel setup)

---

## 📅 SPRINT 3: Frontend - Vacation Calendar (Days 8-11)

**Goal**: Build beautiful vacation calendar with 3 views and exports

**Progress**: ░░░░░░░░ 0/8 tasks complete

### Day 8-9: Calendar Components

#### [ ] Task 3.1: VacationTimelineView Component (Gantt Chart) ⭐

**File**: `frontend/components/vacation/VacationTimelineView.vue`

**📖 Reference**: [Frontend: Vacation Calendar - VacationTimelineView](.ai/frontend/vacation-calendar.md#1-vacationtimelineview-gantt-chart-)

**Requirements**:
- Horizontal timeline (date range on X-axis)
- Employee rows on Y-axis
- Vacation bars positioned by date (CSS: left %, width %)
- Color-coded by status (Scheduled=green, Active=blue, Completed=gray)
- Hover tooltip with details (employee, dates, substitute)
- Click to view vacation details modal

**UI Structure**:
```
┌──────────────┬─────────────────────────────────────────┐
│ Jan Kowalski │ ████████████░░░░░░░░░░░░░░░░░░░░░░░░░░│
│ Anna Nowak   │ ░░░░░░░░░░░░████████░░░░░░░░░░░░░░░░░░│
│ Piotr Wiśnia │ ░░░░░░░░░░░░░░░░░░░░░░░████████████░░░│
└──────────────┴─────────────────────────────────────────┘
               1    5    10   15   20   25   30
```

**Key Logic**:
```typescript
const getVacationBarStyle = (vacation: VacationSchedule) => {
  const totalDays = daysInRange.value.length
  const rangeStart = props.startDate.getTime()
  const dayMs = 1000 * 60 * 60 * 24

  const vacationStart = Math.max(
    new Date(vacation.startDate).getTime(),
    rangeStart
  )
  const vacationEnd = Math.min(
    new Date(vacation.endDate).getTime(),
    props.endDate.getTime()
  )

  const daysFromStart = (vacationStart - rangeStart) / dayMs
  const vacationDays = (vacationEnd - vacationStart) / dayMs + 1

  return {
    left: `${(daysFromStart / totalDays) * 100}%`,
    width: `${(vacationDays / totalDays) * 100}%`
  }
}
```

**Acceptance Criteria**:
- ✅ Vacation bars positioned correctly by date
- ✅ Overlapping vacations visible (stacked vertically)
- ✅ Smooth hover effects with tooltips
- ✅ Responsive (horizontal scroll on mobile)
- ✅ Click opens vacation details modal

**Dependencies**: Task 2.7 (API endpoints)

**Estimated Time**: 6 hours

---

#### [ ] Task 3.2: VacationCalendarGrid Component (Month View)

**File**: `frontend/components/vacation/VacationCalendarGrid.vue`

**📖 Reference**: [Frontend: Vacation Calendar - VacationCalendarGrid](.ai/frontend/vacation-calendar.md#2-vacationcalendargrid-month-view)

**Requirements**:
- Traditional month calendar grid (7 columns x 5-6 rows)
- Show employee names on days they're on vacation
- Click day → modal with vacation details
- Highlight today with border
- Navigate prev/next month
- Show "+X more" badge if >3 people on vacation same day

**UI Structure**:
```
        December 2024
 Mo  Tu  We  Th  Fr  Sa  Su
                    1   2   3
  4   5   6   7   8   9  10
 11  12  13  14  15  16  17
     ┌────────────────┐
     │ Jan K, Anna N  │ ← Vacation badges
     │ Piotr W        │
     │ +2 more        │
     └────────────────┘
```

**Key Logic**:
```typescript
const calendarDays = computed(() => {
  // Generate 35-42 days (5-6 weeks) starting from Monday before month start
  // Mark each day with: isCurrentMonth, isToday, vacations[]
  // ...
})
```

**Acceptance Criteria**:
- ✅ All vacations shown on correct dates
- ✅ Click day shows modal with all vacations
- ✅ Badge shows "+X more" when >3 people
- ✅ Today highlighted
- ✅ Prev/next month navigation works

**Dependencies**: Task 2.7

**Estimated Time**: 5 hours

---

#### [ ] Task 3.3: VacationListView Component (Table)

**File**: `frontend/components/vacation/VacationListView.vue`

**📖 Reference**: [Frontend: Vacation Calendar - VacationListView](.ai/frontend/vacation-calendar.md#3-vacationlistview-table)

**Requirements**:
- Sortable table (by employee, start date, end date, days)
- Search by employee name (debounced)
- Filter by status (Scheduled, Active, Completed)
- Pagination (20 per page)
- Export selected to CSV (optional)

**Columns**:
1. Employee (avatar + name + position)
2. Start Date
3. End Date
4. Days Count
5. Substitute
6. Status (badge)

**Key Features**:
```typescript
const sortBy = (column: string) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}
```

**Acceptance Criteria**:
- ✅ Can sort by any column
- ✅ Search filters results in real-time (debounced 300ms)
- ✅ Pagination works (prev/next, page numbers)
- ✅ Status filter updates list
- ✅ Responsive on mobile (stacked columns)

**Dependencies**: Task 2.7

**Estimated Time**: 4 hours

---

### Day 10: Main Calendar Page

#### [ ] Task 3.4: Team Vacation Calendar Page ⭐

**File**: `frontend/pages/dashboard/team/vacation-calendar.vue`

**📖 Reference**: [Frontend: Vacation Calendar - Main Page](.ai/frontend/vacation-calendar.md#page-structure)

**Requirements**:
- 3 view tabs (Timeline, Calendar, List) - switch between components
- Statistics dashboard at top (3 stat cards)
- Filters (search, status, date range)
- Export buttons (PDF, Excel) - trigger downloads
- Alerts section (conflict warnings)
- Month navigation (prev/next)
- Loading states
- Error handling

**Layout**:
```
┌─────────────────────────────────────────────────┐
│ 📅 Kalendarz urlopów zespołu    [PDF] [Excel]  │
├─────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│ │ 3 osób   │ │ 7 zapl.  │ │ ⚠️ Alert │        │
│ │ na urlopie│ │ urlopów  │ │ 25.12    │        │
│ └──────────┘ └──────────┘ └──────────┘        │
├─────────────────────────────────────────────────┤
│ [Timeline] [Calendar] [List]                    │
├─────────────────────────────────────────────────┤
│ <Selected View Component>                       │
└─────────────────────────────────────────────────┘
```

**Key State Management**:
```typescript
const currentView = ref<'timeline' | 'calendar' | 'list'>('timeline')
const currentMonth = ref(new Date())
const calendarData = ref<VacationCalendar>(...)
```

**Acceptance Criteria**:
- ✅ All 3 views work and switch correctly
- ✅ Statistics update when filters change
- ✅ Alerts shown prominently at top
- ✅ Only managers can access page (middleware: `['auth', 'manager']`)
- ✅ PDF/Excel export downloads correctly
- ✅ Loading spinner shown while fetching data
- ✅ Error message shown on API failure

**Dependencies**: Tasks 3.1, 3.2, 3.3

**Estimated Time**: 4 hours

---

### Day 11: Statistics & Export

#### [ ] Task 3.5: Statistics Dashboard Component

**File**: `frontend/components/vacation/VacationStatistics.vue`

**📖 Reference**: [Frontend: Vacation Calendar - Statistics](.ai/frontend/vacation-calendar.md)

**Requirements**:
- 3 stat cards in grid layout
- Card 1: Currently on vacation (blue)
- Card 2: Scheduled vacations (green)
- Card 3: Alerts (red, only if alerts exist)
- Click alert card → scroll to alerts section
- Responsive (stack on mobile)

**Implementation**:
```vue
<div class="grid grid-cols-1 md:grid-cols-3 gap-4">
  <StatCard
    icon="users"
    :value="stats.currentlyOnVacation"
    label="Obecnie na urlopie"
    :subtitle="`z ${stats.teamSize} pracowników (${coveragePercent}%)`"
    color="blue"
  />
  <StatCard
    icon="calendar"
    :value="stats.scheduledVacations"
    label="Zaplanowanych urlopów"
    subtitle="w tym miesiącu"
    color="green"
  />
  <StatCard
    v-if="alerts.length > 0"
    icon="alert-triangle"
    :value="alerts.length"
    label="Alerty kolizji"
    :subtitle="alerts[0].message"
    color="red"
    clickable
    @click="$emit('alert-click')"
  />
</div>
```

**Acceptance Criteria**:
- ✅ Stats calculated correctly from API data
- ✅ Updates in real-time when data changes
- ✅ Responsive on mobile (stacks vertically)
- ✅ Alert card clickable (emits event)

**Dependencies**: Task 3.4

**Estimated Time**: 2 hours

---

#### [ ] Task 3.6: Vacation Alerts Component

**File**: `frontend/components/vacation/VacationAlerts.vue`

**📖 Reference**: [Frontend: Vacation Calendar - Alerts](.ai/frontend/vacation-calendar.md)

**Requirements**:
- List of conflict alerts
- Red background for critical (>50%)
- Yellow for warnings (>30%)
- Show: date, affected employees, coverage %
- Expandable (show first 3 alerts, "Show all" button)

**UI**:
```vue
<div
  :class="[
    'rounded-lg p-4 border',
    alert.type === 'COVERAGE_CRITICAL'
      ? 'bg-red-50 border-red-200 dark:bg-red-900/20 dark:border-red-800'
      : 'bg-yellow-50 border-yellow-200 dark:bg-yellow-900/20 dark:border-yellow-800'
  ]"
>
  <div class="flex items-start gap-3">
    <AlertTriangle class="w-5 h-5 text-red-600" />
    <div>
      <p class="font-semibold">{{ alert.message }}</p>
      <p class="text-sm">
        {{ formatDate(alert.date) }}:
        {{ alert.affectedEmployees.map(e => e.firstName).join(', ') }}
      </p>
    </div>
  </div>
</div>
```

**Acceptance Criteria**:
- ✅ Alerts sorted by date (earliest first)
- ✅ Critical alerts shown before warnings
- ✅ Can click to see affected employees details
- ✅ "Show all" expands to show all alerts

**Dependencies**: Task 3.4

**Estimated Time**: 2 hours

---

#### [ ] Task 3.7: Export to PDF

**Backend**: Task 2.7 already implements endpoint
**Frontend**: Trigger download from button

**File**: `frontend/pages/dashboard/team/vacation-calendar.vue` (add export handler)

**📖 Reference**: [Frontend: Vacation Calendar - Export](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Button triggers API call to `/api/vacation-schedules/export/pdf`
- Response is blob (PDF file)
- Download automatically with filename `kalendarz-urlopow-{year}-{month}.pdf`
- Show loading spinner during download
- Show error toast if download fails

**Implementation**:
```typescript
const handleExportPdf = async () => {
  try {
    loading.value = true
    const year = currentMonth.value.getFullYear()
    const month = currentMonth.value.getMonth() + 1

    const blob = await exportToPdf(
      authStore.user!.departmentId,
      year,
      month
    )

    downloadFile(blob, `kalendarz-urlopow-${year}-${month.toString().padStart(2, '0')}.pdf`)

    toast.success('Kalendarz wyeksportowany do PDF')
  } catch (err) {
    toast.error('Nie udało się wyeksportować do PDF')
  } finally {
    loading.value = false
  }
}
```

**Acceptance Criteria**:
- ✅ PDF generates without errors
- ✅ Downloads automatically (not opens in new tab)
- ✅ Filename correct format
- ✅ PDF contains: team name, month, statistics, vacation list

**Dependencies**: Task 2.7 (backend PDF generation)

**Estimated Time**: 1 hour

---

#### [ ] Task 3.8: Export to Excel

**Backend**: Task 2.7 already implements endpoint
**Frontend**: Trigger download from button

**File**: `frontend/pages/dashboard/team/vacation-calendar.vue` (add export handler)

**📖 Reference**: [Frontend: Vacation Calendar - Export](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Button triggers API call to `/api/vacation-schedules/export/excel`
- Response is blob (Excel file)
- Download automatically with filename `kalendarz-urlopow-{year}-{month}.xlsx`
- Excel has multiple sheets: Vacations, Statistics, Alerts

**Implementation**:
```typescript
const handleExportExcel = async () => {
  try {
    loading.value = true
    const year = currentMonth.value.getFullYear()
    const month = currentMonth.value.getMonth() + 1

    const blob = await exportToExcel(
      authStore.user!.departmentId,
      year,
      month
    )

    downloadFile(blob, `kalendarz-urlopow-${year}-${month.toString().padStart(2, '0')}.xlsx`)

    toast.success('Kalendarz wyeksportowany do Excel')
  } catch (err) {
    toast.error('Nie udało się wyeksportować do Excel')
  } finally {
    loading.value = false
  }
}
```

**Acceptance Criteria**:
- ✅ Excel generates without errors
- ✅ Can open in Microsoft Excel / LibreOffice
- ✅ Multiple sheets: Vacations (list), Statistics, Alerts
- ✅ Proper formatting (headers bold, dates formatted)

**Dependencies**: Task 2.7 (backend Excel generation)

**Estimated Time**: 1 hour

---

## 📅 SPRINT 4: Frontend - Structure & Requests (Days 12-14)

**Goal**: Build organizational structure management and fix request UX

**Progress**: ░░░░░░░ 0/7 tasks complete

### Day 12: Organizational Structure

#### [ ] Task 4.1: DepartmentTree Component

**File**: `frontend/components/admin/DepartmentTree.vue`

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md) (organizational structure section)

**Requirements**:
- Recursive tree component (department can have child departments)
- Expand/collapse branches (click arrow icon)
- Show department head (avatar + name)
- Actions: Add child, Edit, Delete (dropdown menu)
- Drag & drop to reorder (optional, advanced)

**UI**:
```
📁 PortalForge Inc.
  └─ 👤 Jan Kowalski (President)
  ├─ 📁 IT Department
  │   └─ 👤 Piotr Wiśniewski (Director)
  │   ├─ 📁 Development
  │   │   └─ 👤 Maria Kowalczyk (Manager)
  │   │   ├─ 👨‍💻 Krzysztof Wójcik
  │   │   └─ 👩‍💻 Magdalena Kamińska
  │   └─ 📁 Infrastructure
  └─ 📁 HR Department
```

**Recursive Logic**:
```vue
<template>
  <div class="department-tree-item">
    <div class="flex items-center gap-2">
      <button @click="expanded = !expanded" v-if="department.children.length > 0">
        <ChevronRight :class="{ 'rotate-90': expanded }" />
      </button>

      <Folder class="w-5 h-5" />
      <span>{{ department.name }}</span>

      <!-- Department head -->
      <UserAvatar v-if="department.headOfDepartment" :user="department.headOfDepartment" />

      <!-- Actions -->
      <DropdownMenu>
        <DropdownMenuItem @click="$emit('add-child', department)">Add child</DropdownMenuItem>
        <DropdownMenuItem @click="$emit('edit', department)">Edit</DropdownMenuItem>
        <DropdownMenuItem @click="$emit('delete', department)">Delete</DropdownMenuItem>
      </DropdownMenu>
    </div>

    <!-- Recursive children -->
    <div v-if="expanded && department.children.length > 0" class="ml-6">
      <DepartmentTree
        v-for="child in department.children"
        :key="child.id"
        :department="child"
        @add-child="$emit('add-child', $event)"
        @edit="$emit('edit', $event)"
        @delete="$emit('delete', $event)"
      />
    </div>
  </div>
</template>
```

**Acceptance Criteria**:
- ✅ Shows full tree hierarchy (all levels)
- ✅ Can expand/collapse branches
- ✅ Actions work (emits events)
- ✅ Department head shown if assigned
- ✅ Responsive (collapses on mobile)

**Dependencies**: Task 2.6 (Department API)

**Estimated Time**: 4 hours

---

#### [ ] Task 4.2: Admin Structure Management Page

**File**: `frontend/pages/admin/structure/index.vue`

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Display DepartmentTree component
- Button: "Add Root Department"
- Modal for creating/editing department (form with name, description, parent, head)
- Assign department head via UserAutocomplete
- Delete confirmation modal

**Layout**:
```
┌────────────────────────────────────────┐
│ Struktura organizacyjna      [+ Dodaj] │
├────────────────────────────────────────┤
│ <DepartmentTree />                     │
└────────────────────────────────────────┘
```

**Create/Edit Modal**:
```vue
<Modal v-if="showModal">
  <h2>{{ editingDepartment ? 'Edytuj' : 'Dodaj' }} dział</h2>

  <Input v-model="form.name" label="Nazwa" required />
  <Textarea v-model="form.description" label="Opis" />
  <DepartmentSelect v-model="form.parentDepartmentId" label="Dział nadrzędny" />
  <UserAutocomplete v-model="form.headOfDepartmentId" label="Szef działu" />

  <Button @click="saveDepartment">Zapisz</Button>
</Modal>
```

**Acceptance Criteria**:
- ✅ Only Admin can access (middleware: `['auth', 'admin']`)
- ✅ Can create department hierarchy
- ✅ Can assign department head (autocomplete users)
- ✅ Can edit department (name, description, head)
- ✅ Can delete department (soft delete, confirmation required)
- ✅ Changes reflected immediately in tree

**Dependencies**: Tasks 2.6, 4.1

**Estimated Time**: 4 hours

---

### Day 13: Request UX Improvements

#### [ ] Task 4.3: Fix Clickable Requests ⭐ CRITICAL

**File**: `frontend/pages/dashboard/requests/index.vue`

**📖 Reference**: [Frontend: Vacation Calendar - Fix Clickable Requests](.ai/frontend/vacation-calendar.md)

**Problem**: Requests are in `<button>` tags - not real links, can't right-click "Open in new tab"

**Solution**: Replace `<button @click="viewRequest">` with `<NuxtLink :to="...">`

**Before**:
```vue
<button
  @click="viewRequestDetails(request)"
  class="w-full bg-white rounded-lg shadow p-6 hover:shadow-lg"
>
  <!-- Request card content -->
</button>
```

**After**:
```vue
<NuxtLink
  :to="`/dashboard/requests/${request.id}`"
  class="block bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 hover:border-blue-500 dark:hover:border-blue-400 transition-all p-6"
>
  <!-- Request card content (same) -->
</NuxtLink>
```

**Acceptance Criteria**:
- ✅ Requests are clickable links (not buttons)
- ✅ Right-click → "Open in new tab" works
- ✅ Hover effect shows it's clickable (cursor: pointer)
- ✅ Navigation works correctly to /dashboard/requests/{id}
- ✅ Keyboard navigation works (Tab, Enter)

**Dependencies**: None

**Estimated Time**: 30 minutes

---

#### [ ] Task 4.4: Request Details Page

**File**: `frontend/pages/dashboard/requests/[id].vue` (NEW FILE)

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Show full request details
- Timeline of approval steps (visual component)
- Filled form data (formatted nicely, not raw JSON)
- Action buttons (Approve/Reject) if user is current approver
- Back button to request list
- Loading state
- 404 if request not found

**Layout**:
```
┌────────────────────────────────────────┐
│ ← Back to Requests                     │
├────────────────────────────────────────┤
│ Wniosek urlopowy (#REQ-2024-001)      │
│ Status: W trakcie oceny                │
│                                         │
│ ┌──────────────────────────────────┐  │
│ │ Informacje podstawowe             │  │
│ │ Numer: REQ-2024-001               │  │
│ │ Typ: Wniosek urlopowy             │  │
│ │ Data złożenia: 01.12.2024         │  │
│ │ Wnioskodawca: Jan Kowalski        │  │
│ └──────────────────────────────────┘  │
│                                         │
│ ┌──────────────────────────────────┐  │
│ │ Timeline akceptacji               │  │
│ │ ✅ Step 1: Kierownik (zatw.)      │  │
│ │ ⏳ Step 2: Dyrektor (w trakcie)   │  │
│ │ ⏸️ Step 3: HR (oczekuje)          │  │
│ └──────────────────────────────────┘  │
│                                         │
│ ┌──────────────────────────────────┐  │
│ │ Wypełniony formularz              │  │
│ │ Data od: 15.12.2024               │  │
│ │ Data do: 20.12.2024               │  │
│ │ Liczba dni: 6                     │  │
│ │ Zastępca: Anna Nowak              │  │
│ └──────────────────────────────────┘  │
│                                         │
│ [Zatwierdź] [Odrzuć]                  │
└────────────────────────────────────────┘
```

**Key Logic**:
```typescript
const route = useRoute()
const requestId = route.params.id as string

const request = ref<Request | null>(null)
const loading = ref(true)

const isCurrentApprover = computed(() => {
  const currentStep = request.value?.approvalSteps.find(s => s.status === 'InReview')
  return currentStep?.approverId === authStore.user?.id
})
```

**Acceptance Criteria**:
- ✅ Shows all request data correctly
- ✅ Timeline is clear and visual (icons for status)
- ✅ Form data formatted nicely (key-value pairs, not JSON blob)
- ✅ Action buttons only shown to current approver
- ✅ Approve/Reject modals work
- ✅ Back button navigates correctly

**Dependencies**: Existing request API

**Estimated Time**: 5 hours

---

### Day 14: Template & Substitution

#### [ ] Task 4.5: UserAutocomplete Component

**File**: `frontend/components/common/UserAutocomplete.vue`

**📖 Reference**: [Frontend: Vacation Calendar - UserAutocomplete](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Autocomplete search for users
- Filter by department (optional prop)
- Show: avatar + full name + position
- Debounced search (300ms)
- Return selected user (emit event)
- Clear selection button

**Implementation**:
```vue
<template>
  <div class="relative">
    <input
      v-model="searchQuery"
      @input="debouncedSearch"
      placeholder="Wpisz imię lub nazwisko..."
      class="w-full px-4 py-2 border rounded"
    />

    <!-- Dropdown results -->
    <div v-if="results.length > 0" class="absolute z-10 w-full bg-white shadow-lg rounded-lg">
      <div
        v-for="user in results"
        :key="user.id"
        @click="selectUser(user)"
        class="flex items-center gap-3 p-3 hover:bg-gray-100 cursor-pointer"
      >
        <UserAvatar :user="user" size="sm" />
        <div>
          <p class="font-medium">{{ user.fullName }}</p>
          <p class="text-sm text-gray-500">{{ user.position }}</p>
        </div>
      </div>
    </div>

    <!-- Selected user -->
    <div v-if="selectedUser" class="mt-2 flex items-center gap-3 p-3 bg-blue-50 rounded">
      <UserAvatar :user="selectedUser" size="sm" />
      <span>{{ selectedUser.fullName }}</span>
      <button @click="clearSelection">✕</button>
    </div>
  </div>
</template>
```

**Acceptance Criteria**:
- ✅ Searches as user types (debounced 300ms)
- ✅ Shows max 10 results
- ✅ Emits selected user (`@select="handleSelect"`)
- ✅ Can clear selection
- ✅ Filters by department if prop provided
- ✅ Keyboard navigation (arrow keys, Enter to select)

**Dependencies**: User API endpoint (GET /api/users/search?q=...)

**Estimated Time**: 3 hours

---

#### [ ] Task 4.6: Update Request Template Form - Substitute Field

**File**: `frontend/pages/admin/request-templates/create.vue`

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Add checkbox "Wymaga wyboru zastępcy" (RequiresSubstituteSelection)
- When checked, automatically add field to template: "Wybierz zastępcę" (UserSelect type)
- Field should be marked as required
- This field will be used in vacation requests

**Form Addition**:
```vue
<Checkbox
  v-model="form.requiresSubstituteSelection"
  label="Wymaga wyboru zastępcy (np. dla wniosków urlopowych)"
/>

<!-- Auto-add substitute field if checked -->
<div v-if="form.requiresSubstituteSelection" class="mt-4 p-4 bg-blue-50 rounded">
  <p class="text-sm text-blue-800">
    Pole "Wybierz zastępcę" zostanie automatycznie dodane do formularza.
  </p>
</div>
```

**Backend Integration**:
```typescript
// When creating template
const template = {
  // ... other fields
  requiresSubstituteSelection: form.requiresSubstituteSelection,
  fields: [
    ...form.fields,
    ...(form.requiresSubstituteSelection ? [{
      name: 'substitute',
      label: 'Wybierz zastępcę',
      type: 'UserSelect',
      required: true,
      order: 999
    }] : [])
  ]
}
```

**Acceptance Criteria**:
- ✅ Checkbox appears in template builder
- ✅ Field automatically added to vacation templates
- ✅ When user submits vacation request, they must select substitute
- ✅ Substitute field uses UserAutocomplete component

**Dependencies**: Tasks 4.5, existing template creation

**Estimated Time**: 2 hours

---

#### [ ] Task 4.7: My Substitutions Panel

**File**: `frontend/pages/dashboard/substitutions/index.vue` (NEW FILE)

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Requirements**:
- Show active substitutions (who you're currently substituting)
- Show scheduled substitutions (upcoming)
- Show completed substitutions (past, last 5)
- Link to pending approvals for each active substitution
- Empty state if no substitutions

**Layout**:
```
┌────────────────────────────────────────┐
│ Moje zastępstwa                        │
├────────────────────────────────────────┤
│ 🟢 Aktywne zastępstwa (2)              │
│ ┌──────────────────────────────────┐  │
│ │ Zastępujesz: Jan Kowalski         │  │
│ │ Od: 15.12.2024  Do: 20.12.2024    │  │
│ │ [Zobacz wnioski do zatwierdzenia] │  │
│ └──────────────────────────────────┘  │
│ ┌──────────────────────────────────┐  │
│ │ Zastępujesz: Anna Nowak           │  │
│ │ Od: 18.12.2024  Do: 22.12.2024    │  │
│ │ [Zobacz wnioski (0)]              │  │
│ └──────────────────────────────────┘  │
│                                         │
│ 🔵 Zaplanowane zastępstwa (1)          │
│ ┌──────────────────────────────────┐  │
│ │ Będziesz zastępować: Piotr Wiśnia │  │
│ │ Od: 25.12.2024  Do: 30.12.2024    │  │
│ └──────────────────────────────────┘  │
└────────────────────────────────────────┘
```

**API Call**:
```typescript
const { getMySubstitutions } = useVacationApi()
const substitutions = await getMySubstitutions()

const activeSubstitutions = computed(() =>
  substitutions.value.filter(s => s.status === 'Active')
)
const scheduledSubstitutions = computed(() =>
  substitutions.value.filter(s => s.status === 'Scheduled')
)
```

**Acceptance Criteria**:
- ✅ Shows current substitutions prominently
- ✅ Shows upcoming substitutions
- ✅ Link to approvals works (filters by person being substituted)
- ✅ Empty state if no substitutions
- ✅ Badge shows count of pending approvals per substitution

**Dependencies**: Task 2.7 (vacation API)

**Estimated Time**: 3 hours

---

## 📅 SPRINT 5: Permissions & Notifications (Days 15-16)

**Goal**: Implement visibility permissions and enhance notifications

**Progress**: ░░░░░ 0/5 tasks complete

### Day 15: Organizational Permissions

#### [ ] Task 5.1: Organizational Permission Endpoints

**File**: `backend/PortalForge.Api/Controllers/PermissionsController.cs` (NEW)

**📖 Reference**: [Backend: Organizational Structure](.ai/backend/organizational-structure.md)

**Endpoints**:
- `GET /api/permissions/organizational/{userId}` - Get user permissions
- `PUT /api/permissions/organizational/{userId}` - Update (Admin only)

**Implementation**:
```csharp
[HttpGet("organizational/{userId}")]
[Authorize]
public async Task<IActionResult> GetOrganizationalPermission(Guid userId)
{
    // Users can view their own permissions, admins can view any
    if (userId != CurrentUser.Id && !CurrentUser.IsAdmin)
        return Forbid();

    var permission = await _context.OrganizationalPermissions
        .FirstOrDefaultAsync(p => p.UserId == userId);

    if (permission == null)
    {
        // Default: user can only see their department
        return Ok(new OrganizationalPermissionDto
        {
            UserId = userId,
            CanViewAllDepartments = false,
            VisibleDepartmentIds = new List<Guid> { CurrentUser.DepartmentId }
        });
    }

    return Ok(MapToDto(permission));
}

[HttpPut("organizational/{userId}")]
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> UpdateOrganizationalPermission(
    Guid userId,
    [FromBody] UpdateOrganizationalPermissionCommand command)
{
    command.UserId = userId;
    await _mediator.Send(command);
    return NoContent();
}
```

**Acceptance Criteria**:
- ✅ Can get user's permissions
- ✅ Can grant/revoke department visibility (admin only)
- ✅ Default permission created if not exists
- ✅ Validation: can't grant permissions for non-existent departments

**Dependencies**: Task 1.3, 1.8

**Estimated Time**: 2 hours

---

#### [ ] Task 5.2: Admin Panel - Grant Department Visibility

**File**: `frontend/pages/admin/permissions/organizational.vue` (NEW)

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Requirements**:
- List of users (paginated, searchable)
- For each user: checkbox "Can view all departments"
- Multi-select for specific departments (if not "all")
- Save button (per user or batch)
- Loading state while saving

**UI**:
```
┌────────────────────────────────────────┐
│ Uprawnienia widoczności działów        │
│ [Search users...]                      │
├────────────────────────────────────────┤
│ Jan Kowalski                           │
│ ☐ Może widzieć wszystkie działy       │
│ Dostępne działy:                       │
│ ☑ IT Department                        │
│ ☑ Development                          │
│ ☐ HR Department                        │
│ ☐ Finance Department                   │
│ [Zapisz]                               │
├────────────────────────────────────────┤
│ Anna Nowak                             │
│ ☑ Może widzieć wszystkie działy       │
│ [Zapisz]                               │
└────────────────────────────────────────┘
```

**Key Logic**:
```typescript
const savePermissions = async (user: User) => {
  const permission = {
    userId: user.id,
    canViewAllDepartments: user.canViewAllDepartments,
    visibleDepartmentIds: user.canViewAllDepartments
      ? []
      : user.selectedDepartments.map(d => d.id)
  }

  await updateOrganizationalPermission(user.id, permission)
  toast.success('Uprawnienia zaktualizowane')
}
```

**Acceptance Criteria**:
- ✅ Can select multiple departments per user
- ✅ "All departments" checkbox disables department select
- ✅ Changes save correctly to backend
- ✅ User can immediately see permitted departments (test by logging in as user)
- ✅ Only Admin can access page

**Dependencies**: Task 5.1

**Estimated Time**: 4 hours

---

### Day 16: Notification Enhancements

#### [ ] Task 5.3: Notification Service - Vacation Reminders

**File**: `backend/PortalForge.Application/Services/NotificationService.cs` (extend existing)

**📖 Reference**: [Backend: Vacation System](.ai/backend/vacation-schedule-system.md)

**New Methods**:
```csharp
Task NotifySubstituteAsync(Guid substituteId, VacationSchedule vacation);
Task NotifyVacationStartingSoonAsync(Guid userId, VacationSchedule vacation);
Task NotifyVacationEndedAsync(Guid userId, VacationSchedule vacation);
```

**Triggers**:
- **1 day before vacation**: Notify user & substitute
- **On vacation start**: Notify substitute (via daily job)
- **On vacation end**: Notify user (via daily job)

**Implementation**:
```csharp
public async Task NotifySubstituteAsync(Guid substituteId, VacationSchedule vacation)
{
    await CreateNotificationAsync(
        substituteId,
        NotificationType.VacationSubstitution,
        "Nowe zastępstwo",
        $"Od {vacation.StartDate:dd.MM} będziesz zastępować {vacation.User.FullName}",
        relatedEntityType: "VacationSchedule",
        relatedEntityId: vacation.Id.ToString(),
        actionUrl: "/dashboard/substitutions"
    );
}
```

**Acceptance Criteria**:
- ✅ Notifications sent at correct times
- ✅ Substitute knows who they're covering and when
- ✅ User reminded about upcoming vacation (1 day before)
- ✅ User notified when vacation ends

**Dependencies**: Task 2.3, 2.5 (daily job)

**Estimated Time**: 2 hours

---

#### [ ] Task 5.4: Email Notifications

**File**: `backend/PortalForge.Infrastructure/Services/EmailService.cs` (NEW)

**📖 Reference**: [Backend: Vacation System](.ai/backend/vacation-schedule-system.md)

**Requirements**:
- Send email for critical notifications
- Template-based HTML emails
- Use SendGrid or SMTP (configurable)
- Unsubscribe link (optional)

**Email Types**:
1. Request pending approval
2. Request approved/rejected
3. Vacation starting soon (1 day before)
4. You're substituting someone

**Configuration** (appsettings.json):
```json
{
  "Email": {
    "Provider": "SendGrid", // or "SMTP"
    "SendGridApiKey": "...",
    "FromEmail": "noreply@portalforge.com",
    "FromName": "PortalForge"
  }
}
```

**Example Template** (vacation-reminder.html):
```html
<h2>Przypomnienie o urlopie</h2>
<p>Cześć {{ userName }},</p>
<p>Jutro rozpoczyna się Twój urlop ({{ startDate }} - {{ endDate }}).</p>
<p>Twój zastępca: <strong>{{ substituteName }}</strong></p>
<a href="{{ appUrl }}/dashboard/substitutions">Zobacz szczegóły</a>
```

**Acceptance Criteria**:
- ✅ Emails sent successfully (test with Mailtrap or real email)
- ✅ HTML formatting looks professional
- ✅ Links in email work correctly
- ✅ Handles errors gracefully (logs but doesn't crash app)

**Dependencies**: Task 5.3

**Estimated Time**: 4 hours

---

#### [ ] Task 5.5: NotificationBell Improvements

**File**: `frontend/components/NotificationBell.vue` (update existing)

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Enhancements**:
- Real-time updates (polling every 30s)
- Mark all as read button
- Group by type (Requests, Vacations, System)
- Unread badge on bell icon
- Click notification → navigate to relevant page

**Implementation**:
```typescript
// Polling for new notifications
const pollInterval = ref<NodeJS.Timeout | null>(null)

onMounted(() => {
  loadNotifications()

  pollInterval.value = setInterval(() => {
    loadNotifications()
  }, 30000) // 30 seconds
})

onUnmounted(() => {
  if (pollInterval.value) {
    clearInterval(pollInterval.value)
  }
})
```

**Grouped Display**:
```vue
<div class="notification-dropdown">
  <div class="notification-header">
    <h3>Powiadomienia</h3>
    <button @click="markAllAsRead">Oznacz wszystkie jako przeczytane</button>
  </div>

  <!-- Group: Requests -->
  <div v-if="requestNotifications.length > 0">
    <h4 class="text-xs font-semibold text-gray-500 uppercase px-4 py-2">Wnioski</h4>
    <NotificationItem
      v-for="notif in requestNotifications"
      :key="notif.id"
      :notification="notif"
      @click="handleNotificationClick(notif)"
    />
  </div>

  <!-- Group: Vacations -->
  <div v-if="vacationNotifications.length > 0">
    <h4 class="text-xs font-semibold text-gray-500 uppercase px-4 py-2">Urlopy</h4>
    <NotificationItem
      v-for="notif in vacationNotifications"
      :key="notif.id"
      :notification="notif"
      @click="handleNotificationClick(notif)"
    />
  </div>

  <div class="notification-footer">
    <NuxtLink to="/dashboard/notifications">Zobacz wszystkie</NuxtLink>
  </div>
</div>
```

**Acceptance Criteria**:
- ✅ Badge shows unread count
- ✅ Dropdown shows latest 10 notifications
- ✅ Grouped by type for better organization
- ✅ "View all" link goes to /dashboard/notifications
- ✅ Auto-refreshes every 30s
- ✅ Click notification → navigates to actionUrl

**Dependencies**: Existing notification system

**Estimated Time**: 3 hours

---

## 📅 SPRINT 6: Testing & Documentation (Days 17-18)

**Goal**: Comprehensive testing and documentation updates

**Progress**: ░░░░░░ 0/6 tasks complete

### Day 17: Backend Testing

#### [ ] Task 6.1: Unit Tests - RequestRoutingService

**File**: `backend/PortalForge.Tests/Unit/Services/RequestRoutingServiceTests.cs`

**📖 Reference**: [Backend: Organizational Structure - Testing](.ai/backend/organizational-structure.md#testing)

**Test Cases**:
1. ✅ `ResolveByRole_FindsManager_WhenSubmitterHasManagerSupervisor`
2. ✅ `ResolveByRole_FindsDirector_WhenSubmitterHasDirectorInChain`
3. ✅ `ResolveByRole_ReturnsNull_WhenPresidentHasNoSupervisor`
4. ✅ `ResolveByDirectSupervisor_ReturnsImmediateSupervisor`
5. ✅ `ResolveBySpecificDepartment_ReturnsDepartmentHead`
6. ✅ `ResolveBySpecificUser_ReturnsSpecificUser`
7. ✅ `ResolveByUserGroup_ReturnsFirstAvailableFromGroup`
8. ✅ `HasHigherSupervisor_ReturnsTrue_ForEmployeeWithManager`
9. ✅ `HasHigherSupervisor_ReturnsFalse_ForPresident`

**Example Test**:
```csharp
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
```

**Coverage Target**: 80%+

**Dependencies**: Task 2.1

**Estimated Time**: 4 hours

---

#### [ ] Task 6.2: Unit Tests - VacationScheduleService

**File**: `backend/PortalForge.Tests/Unit/Services/VacationScheduleServiceTests.cs`

**📖 Reference**: [Backend: Vacation System - Testing](.ai/backend/vacation-schedule-system.md#testing)

**Test Cases**:
1. ✅ `CreateFromApprovedRequest_CreatesSchedule_WithCorrectDates`
2. ✅ `GetActiveSubstitute_ReturnsSubstitute_WhenUserOnVacation`
3. ✅ `GetActiveSubstitute_ReturnsNull_WhenUserNotOnVacation`
4. ✅ `DetectConflicts_AlertsWhenMoreThan30PercentOnVacation`
5. ✅ `DetectConflicts_CriticalAlertWhenMoreThan50Percent`
6. ✅ `CalculateStatistics_CountsCorrectly`
7. ✅ `UpdateVacationStatuses_ActivatesScheduledVacations`
8. ✅ `UpdateVacationStatuses_CompletesActiveVacations`
9. ✅ `CreateFromApprovedRequest_ThrowsException_WhenSubstituteIsSelf`

**Example Test**:
```csharp
[Fact]
public async Task DetectConflicts_AlertsWhenMoreThan30PercentOnVacation()
{
    // Arrange
    var teamSize = 10;
    var vacations = new List<VacationSchedule>
    {
        new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) },
        new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) },
        new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) },
        new() { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(5) }
        // 4 out of 10 = 40%
    };

    // Act
    var alerts = _service.DetectConflicts(vacations, teamSize, DateTime.Today, DateTime.Today.AddDays(5));

    // Assert
    alerts.Should().NotBeEmpty();
    alerts[0].Type.Should().Be(AlertType.COVERAGE_LOW);
    alerts[0].CoveragePercent.Should().Be(40);
}
```

**Coverage Target**: 80%+

**Dependencies**: Task 2.3

**Estimated Time**: 4 hours

---

#### [ ] Task 6.3: Integration Tests - Department CRUD

**File**: `backend/PortalForge.Tests/Integration/DepartmentsControllerTests.cs`

**📖 Reference**: [Backend: Organizational Structure](.ai/backend/organizational-structure.md)

**Test Cases**:
1. ✅ `GET_Departments_ReturnsTreeStructure`
2. ✅ `POST_Department_CreatesSuccessfully_WhenAdminRole`
3. ✅ `POST_Department_ReturnsForbidden_WhenNotAdmin`
4. ✅ `PUT_Department_UpdatesSuccessfully`
5. ✅ `DELETE_Department_SoftDeletes`
6. ✅ `POST_Department_ValidatesParentExists`
7. ✅ `POST_Department_PreventsCircularReferences`

**Example Test**:
```csharp
[Fact]
public async Task POST_Department_PreventsCircularReferences()
{
    // Arrange
    var parentId = await CreateDepartment("Parent");
    var childId = await CreateDepartment("Child", parentId);

    // Act - Try to make parent a child of child (circular)
    var response = await _client.PutAsJsonAsync(
        $"/api/departments/{parentId}",
        new { ParentDepartmentId = childId }
    );

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var error = await response.Content.ReadAsStringAsync();
    error.Should().Contain("circular reference");
}
```

**Dependencies**: Task 2.6

**Estimated Time**: 3 hours

---

### Day 18: Frontend Testing & Documentation

#### [ ] Task 6.4: E2E Tests - Organizational Structure

**File**: `frontend/tests/e2e/organizational-structure.spec.ts`

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Test Scenarios**:
```typescript
test('Admin can create department hierarchy', async ({ page }) => {
  // 1. Login as admin
  await loginAsAdmin(page)

  // 2. Navigate to /admin/structure
  await page.goto('/admin/structure')

  // 3. Click "Add Department"
  await page.click('button:has-text("Dodaj dział")')

  // 4. Fill form
  await page.fill('input[name="name"]', 'IT Department')
  await page.fill('textarea[name="description"]', 'Information Technology')

  // 5. Click "Save"
  await page.click('button:has-text("Zapisz")')

  // 6. Verify department appears in tree
  await expect(page.locator('text=IT Department')).toBeVisible()
})

test('Admin can assign department head', async ({ page }) => {
  await loginAsAdmin(page)
  await page.goto('/admin/structure')

  // Click edit on existing department
  await page.click('[data-testid="edit-department-btn"]')

  // Search for user to assign as head
  await page.fill('input[placeholder="Wpisz imię lub nazwisko..."]', 'Jan')
  await page.click('text=Jan Kowalski')

  // Save
  await page.click('button:has-text("Zapisz")')

  // Verify user shown as head
  await expect(page.locator('text=Jan Kowalski (Szef działu)')).toBeVisible()
})
```

**Acceptance Criteria**:
- ✅ All E2E tests pass in Playwright
- ✅ Screenshots captured on failure
- ✅ Tests run in CI/CD pipeline

**Dependencies**: Tasks 4.1, 4.2

**Estimated Time**: 3 hours

---

#### [ ] Task 6.5: E2E Tests - Vacation Calendar

**File**: `frontend/tests/e2e/vacation-calendar.spec.ts`

**📖 Reference**: [Frontend: Vacation Calendar](.ai/frontend/vacation-calendar.md)

**Test Scenarios**:
```typescript
test('Manager can view team vacation calendar', async ({ page }) => {
  // 1. Login as manager
  await loginAsManager(page)

  // 2. Navigate to /dashboard/team/vacation-calendar
  await page.goto('/dashboard/team/vacation-calendar')

  // 3. Verify 3 view tabs present
  await expect(page.locator('button:has-text("Timeline")')).toBeVisible()
  await expect(page.locator('button:has-text("Calendar")')).toBeVisible()
  await expect(page.locator('button:has-text("List")')).toBeVisible()

  // 4. Verify statistics shown
  await expect(page.locator('text=Obecnie na urlopie')).toBeVisible()
  await expect(page.locator('text=Zaplanowanych urlopów')).toBeVisible()

  // 5. Click Timeline tab
  await page.click('button:has-text("Timeline")')

  // 6. Verify Gantt chart visible
  await expect(page.locator('[data-testid="vacation-timeline"]')).toBeVisible()
})

test('Conflict alerts shown when >30% team on vacation', async ({ page }) => {
  // Setup: Seed test data with 5/10 employees on vacation
  await seedVacationData({ employeesOnVacation: 5, totalEmployees: 10 })

  await loginAsManager(page)
  await page.goto('/dashboard/team/vacation-calendar')

  // Verify alert shown
  await expect(page.locator('text=Alerty kolizji')).toBeVisible()
  await expect(page.locator('text=⚠️')).toBeVisible()

  // Verify alert details
  await expect(page.locator('text=5/10 pracowników na urlopie')).toBeVisible()
})
```

**Acceptance Criteria**:
- ✅ All E2E tests pass
- ✅ Calendar views render correctly
- ✅ Export functionality tested

**Dependencies**: Tasks 3.1-3.8

**Estimated Time**: 3 hours

---

#### [ ] Task 6.6: Documentation Updates

**Files**:
- `README.md` (update with new features)
- `.ai/backend/organizational-structure.md` (already created)
- `.ai/backend/vacation-schedule-system.md` (already created)
- `.ai/frontend/vacation-calendar.md` (already created)

**📖 Reference**: All documentation files created in planning phase

**Content Updates for README.md**:

```markdown
## New Features

### Organizational Structure (v2.0)
- Unlimited hierarchical department structure
- Department heads and employee assignments
- Visibility permissions (who can see which departments)
- Auto-routing of approval requests based on hierarchy

### Vacation Management (v2.0)
- Automatic substitute assignment when approver on vacation
- Team vacation calendar (Timeline, Grid, List views)
- Conflict detection (alerts when >30% team on vacation)
- Export to PDF/Excel
- Email notifications for vacation reminders

### Request Routing Improvements (v2.0)
- Multi-level hierarchy support (Employee → Manager → Director → VP → President)
- Auto-approval when no higher supervisor exists
- 6 approver types: Role, Specific User, Group, Department, Direct Supervisor, Submitter
```

**Also Update**:
- Migration guide from v1.0 to v2.0
- API documentation (Swagger)
- Environment variable docs (new settings for email, etc.)

**Acceptance Criteria**:
- ✅ README reflects all new features
- ✅ Migration guide explains database changes
- ✅ Code examples included where helpful
- ✅ Diagrams updated (if applicable)

**Dependencies**: All previous tasks

**Estimated Time**: 2 hours

---

## ✅ Definition of Done

Each task is considered "Done" when:

- [x] **Code written** and compiles without errors
- [x] **Unit tests** written and passing (80%+ coverage for critical components)
- [x] **Integration/E2E tests** passing (where applicable)
- [x] **Code reviewed** (self-review checklist: naming, error handling, logging)
- [x] **Documentation updated** (XML docs for public APIs, README if needed)
- [x] **Tested manually** in browser/Postman (happy path + error cases)
- [x] **No regressions** in existing features (run full test suite)
- [x] **Checkbox marked** `[x]` in this implementation plan
- [x] **Progress updated** at bottom of this file
- [x] **Committed** with meaningful commit message (follow Conventional Commits)

---

## 📊 Progress Tracking

**Update this section as you complete tasks!**

### Sprint Progress Bars

**Sprint 1 (Backend Foundation)**: ░░░░░░░░ 0/8 tasks (0%)
**Sprint 2 (Routing & Vacation)**: ░░░░░░░ 0/7 tasks (0%)
**Sprint 3 (Vacation Calendar)**: ░░░░░░░░ 0/8 tasks (0%)
**Sprint 4 (Structure & Requests)**: ░░░░░░░ 0/7 tasks (0%)
**Sprint 5 (Permissions & Notifications)**: ░░░░░ 0/5 tasks (0%)
**Sprint 6 (Testing & Documentation)**: ░░░░░░ 0/6 tasks (0%)

### Overall Progress

```
Progress: ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0/41 (0%)
```

**Completed Tasks**: 0/41
**In Progress**: 0
**Blocked**: 0
**Not Started**: 41

---

## 🚀 Getting Started

To begin implementation:

1. **Read Documentation**: Start with [ADR-005](.ai/decisions/005-organizational-structure-and-vacation-system.md)
2. **Checkout branch**: `git checkout -b feature/organizational-structure`
3. **Start Sprint 1, Task 1.1**: Create Department entity
4. **Check box when done**: Change `[ ]` to `[x]`
5. **Update progress bars**: Count completed tasks and update percentages
6. **Run tests frequently**: `dotnet test` (backend) and `npm run test` (frontend)
7. **Commit early and often**: Use Conventional Commits format

---

## 📞 Need Help?

- **Architecture questions**: Read [ADR-005](.ai/decisions/005-organizational-structure-and-vacation-system.md)
- **Backend implementation**: See [organizational-structure.md](.ai/backend/organizational-structure.md) and [vacation-schedule-system.md](.ai/backend/vacation-schedule-system.md)
- **Frontend components**: See [vacation-calendar.md](.ai/frontend/vacation-calendar.md)
- **Stuck on a task**: Review acceptance criteria and dependencies
- **Tests failing**: Check test examples in documentation

---

**Let's build something amazing! 🎉**

**Remember**: Update checkboxes `[x]` and progress bars as you go! This helps track progress and keeps everyone aligned.
