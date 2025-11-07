# PortalForge - Organizational Structure System

## System Overview

This document provides comprehensive diagrams for the organizational structure management system in PortalForge.

---

## 1. Organizational Hierarchy Model

```mermaid
graph TD
    CEO[CEO / President<br/>John Smith]

    subgraph "Executive Level"
        CTO[CTO<br/>Tech Division]
        CFO[CFO<br/>Finance Division]
        COO[COO<br/>Operations Division]
        CHRO[CHRO<br/>HR Division]
    end

    subgraph "Management Level"
        DevMgr[Development Manager<br/>Engineering Dept]
        QAMgr[QA Manager<br/>Quality Dept]
        FinMgr[Finance Manager<br/>Accounting Dept]
        OpsMgr[Operations Manager<br/>Logistics Dept]
        HRMgr[HR Manager<br/>HR Dept]
    end

    subgraph "Team Lead Level"
        FrontendLead[Frontend Team Lead]
        BackendLead[Backend Team Lead]
        QALead[QA Team Lead]
    end

    subgraph "Individual Contributors"
        Dev1[Developer 1]
        Dev2[Developer 2]
        QA1[QA Engineer 1]
        QA2[QA Engineer 2]
    end

    CEO --> CTO
    CEO --> CFO
    CEO --> COO
    CEO --> CHRO

    CTO --> DevMgr
    CTO --> QAMgr
    CFO --> FinMgr
    COO --> OpsMgr
    CHRO --> HRMgr

    DevMgr --> FrontendLead
    DevMgr --> BackendLead
    QAMgr --> QALead

    FrontendLead --> Dev1
    BackendLead --> Dev2
    QALead --> QA1
    QALead --> QA2

    style CEO fill:#FF6B6B
    style CTO fill:#4ECDC4
    style DevMgr fill:#95E1D3
    style FrontendLead fill:#C7ECEE
    style Dev1 fill:#E8F8F5
```

---

## 2. Data Model - Entity Relationships

```mermaid
erDiagram
    User ||--o{ Employee : "is"
    Employee ||--o| Department : "belongs to"
    Employee ||--o| Position : "has"
    Employee ||--o{ Employee : "supervises"
    Employee ||--o{ Request : "submits"
    Employee ||--o{ VacationSchedule : "has"

    Department ||--o{ Department : "parent of"
    Department ||--o| User : "department head"
    Department ||--o{ Employee : "contains"
    Department ||--o{ OrganizationalPermission : "visible to"

    Position {
        int PositionId PK
        string Title
        string Description
        int Level
        datetime CreatedAt
        datetime UpdatedAt
    }

    Department {
        int DepartmentId PK
        string Name
        string Description
        int ParentDepartmentId FK
        int DepartmentHeadId FK
        datetime CreatedAt
        datetime UpdatedAt
    }

    Employee {
        string UserId PK
        string FirstName
        string LastName
        string Email
        string PhoneNumber
        int DepartmentId FK
        int PositionId FK
        string SupervisorId FK
        string ProfilePhotoUrl
        datetime HireDate
        bool IsActive
    }

    User {
        string UserId PK
        string Email
        string PasswordHash
        string Role
        bool EmailVerified
        datetime CreatedAt
        datetime LastLogin
    }

    OrganizationalPermission {
        int PermissionId PK
        string UserId FK
        int DepartmentId FK
        bool CanView
        bool CanManage
    }

    Request {
        int RequestId PK
        string RequestNumber
        string SubmitterId FK
        int TemplateId FK
        string Status
        datetime SubmittedAt
    }

    VacationSchedule {
        int VacationId PK
        string EmployeeId FK
        date StartDate
        date EndDate
        string Status
        string SubstituteId FK
    }

    User ||--o{ OrganizationalPermission : "has permissions"
```

---

## 3. Department Tree Query Flow

```mermaid
sequenceDiagram
    participant UI as Frontend UI
    participant API as Department Controller
    participant Handler as GetDepartmentTreeHandler
    participant Repo as Department Repository
    participant DB as Database
    participant Mapper as DTO Mapper

    UI->>API: GET /api/departments/tree
    API->>Handler: Handle(GetDepartmentTreeQuery)

    Handler->>Repo: GetDepartmentTreeAsync()
    Repo->>DB: SELECT * FROM Departments<br/>ORDER BY ParentDepartmentId

    DB-->>Repo: All departments (flat list)

    Repo->>Repo: Build hierarchical structure<br/>(recursive algorithm)

    Note over Repo: Group by ParentDepartmentId<br/>Create parent-child relationships<br/>Build tree recursively

    Repo-->>Handler: Hierarchical DepartmentTree

    Handler->>Repo: For each department:<br/>GetDepartmentEmployeesAsync()
    Repo->>DB: SELECT * FROM Employees<br/>WHERE DepartmentId = ?
    DB-->>Repo: Employees per department
    Repo-->>Handler: Employees data

    Handler->>Mapper: Map to DepartmentTreeDto
    Mapper-->>Handler: DepartmentTreeDto with employees

    Handler-->>API: DepartmentTreeDto
    API-->>UI: JSON response

    UI->>UI: Render tree visualization<br/>(Tree/Departments/List view)
```

---

## 4. Three Visualization Modes

```mermaid
graph LR
    subgraph "User Interface - Dashboard/Organization Page"
        ViewToggle[View Mode Toggle]

        subgraph "Tree View"
            OrgChart[PrimeVue OrganizationChart<br/>Interactive Tree]
            PanZoom[Pan & Zoom Controls<br/>Mousewheel + Drag]
            TreeNodes[Hierarchical Nodes<br/>Department Cards]
        end

        subgraph "Departments View"
            DeptCards[Department Cards<br/>Hierarchical Layout]
            VisualIndent[Visual Indentation<br/>Parent-Child Levels]
            ExpandCollapse[Expand/Collapse<br/>Subdepartments]
        end

        subgraph "List View"
            DataTable[Searchable Table<br/>All Employees]
            Filters[Department Filter<br/>Name Search]
            Pagination[Pagination<br/>50 per page]
        end
    end

    ViewToggle --> OrgChart
    ViewToggle --> DeptCards
    ViewToggle --> DataTable

    OrgChart --> EmployeeModal[Employee Details Modal]
    DeptCards --> EmployeeModal
    DataTable --> EmployeeModal

    EmployeeModal --> QuickEdit[Quick Edit<br/>Update Employee]

    style ViewToggle fill:#4CAF50
    style OrgChart fill:#2196F3
    style DeptCards fill:#FF9800
    style DataTable fill:#9C27B0
```

---

## 5. Organizational Permissions System

```mermaid
sequenceDiagram
    participant User as User (Manager)
    participant Frontend as Frontend
    participant API as Permissions Controller
    participant Handler as GetOrgPermissionHandler
    participant PermRepo as Permission Repository
    participant DeptRepo as Department Repository
    participant DB as Database

    Note over User,DB: Check user's department visibility

    User->>Frontend: Navigate to Organization page
    Frontend->>API: GET /api/permissions/organizational

    API->>Handler: Handle(GetOrganizationalPermissionQuery)

    Handler->>PermRepo: GetUserPermissionsAsync(userId)
    PermRepo->>DB: SELECT * FROM OrganizationalPermissions<br/>WHERE UserId = ?
    DB-->>PermRepo: User's department permissions
    PermRepo-->>Handler: List<OrganizationalPermission>

    alt User has "ViewAllDepartments" flag
        Handler-->>API: All departments visible
    else User has specific department permissions
        Handler->>DeptRepo: GetDepartmentsByIdsAsync(permittedDeptIds)
        DeptRepo->>DB: SELECT * FROM Departments<br/>WHERE DepartmentId IN (...)
        DB-->>DeptRepo: Permitted departments
        DeptRepo-->>Handler: Department list

        Handler->>DeptRepo: Get subdepartments recursively
        DeptRepo->>DB: SELECT * FROM Departments<br/>WHERE ParentDepartmentId IN (...)
        DB-->>DeptRepo: Subdepartments
        DeptRepo-->>Handler: Full department tree
    end

    Handler-->>API: Visible departments
    API-->>Frontend: Department visibility data

    Frontend->>Frontend: Filter organization tree<br/>Show only visible departments

    Frontend-->>User: Display filtered organization
```

---

## 6. Employee Management Flow

```mermaid
stateDiagram-v2
    [*] --> ViewOrganization

    ViewOrganization --> SearchEmployee : Search by name
    ViewOrganization --> FilterDepartment : Filter by department
    ViewOrganization --> ClickEmployee : Click on employee

    ClickEmployee --> EmployeeModal

    EmployeeModal --> ViewDetails : View full details
    EmployeeModal --> EditEmployee : Click Edit
    EmployeeModal --> ViewSubordinates : View subordinates
    EmployeeModal --> ViewSupervisor : View supervisor

    EditEmployee --> QuickEditModal
    QuickEditModal --> UpdateEmployee : Save changes
    UpdateEmployee --> ValidationCheck

    ValidationCheck --> Success : Valid
    ValidationCheck --> ErrorDisplay : Invalid

    Success --> RefreshTree : Update organization tree
    ErrorDisplay --> QuickEditModal : Fix errors

    RefreshTree --> ViewOrganization
    ViewSubordinates --> ViewOrganization
    ViewSupervisor --> ViewOrganization
    ViewDetails --> EmployeeModal

    ViewOrganization --> [*]
```

---

## 7. Department CRUD Operations

```mermaid
sequenceDiagram
    participant Admin as Admin User
    participant UI as Admin Panel
    participant API as Departments Controller
    participant Handler as Command Handler
    participant Validator as Validator
    participant UoW as Unit of Work
    participant Repo as Department Repo
    participant DB as Database
    participant Audit as Audit Service

    Note over Admin,Audit: Create Department

    Admin->>UI: Fill create department form
    UI->>API: POST /api/departments
    API->>Handler: Handle(CreateDepartmentCommand)

    Handler->>Validator: ValidateAsync(command)

    alt Validation Fails
        Validator-->>Handler: ValidationException
        Handler-->>UI: 400 Bad Request
    end

    Validator-->>Handler: Valid

    Handler->>UoW: BeginTransaction()
    Handler->>Repo: CreateAsync(department)
    Repo->>DB: INSERT INTO Departments
    DB-->>Repo: DepartmentId
    Repo-->>Handler: Department created

    Handler->>Audit: LogAsync("CREATE", "Department", departmentId)
    Audit->>DB: INSERT INTO AuditLogs
    DB-->>Audit: Success

    Handler->>UoW: CommitTransaction()
    UoW-->>Handler: Success

    Handler-->>API: DepartmentDto
    API-->>UI: 201 Created
    UI-->>Admin: Success message + refresh tree

    Note over Admin,Audit: Update Department

    Admin->>UI: Edit department
    UI->>API: PUT /api/departments/{id}
    API->>Handler: Handle(UpdateDepartmentCommand)

    Handler->>Validator: ValidateAsync(command)
    Validator-->>Handler: Valid

    Handler->>Repo: GetByIdAsync(id)
    Repo->>DB: SELECT
    DB-->>Repo: Department

    alt Department Not Found
        Repo-->>Handler: null
        Handler-->>UI: 404 Not Found
    end

    Handler->>Repo: UpdateAsync(department)
    Repo->>DB: UPDATE Departments
    DB-->>Repo: Success

    Handler->>Audit: LogAsync("UPDATE", "Department", id)
    Handler-->>API: Updated DepartmentDto
    API-->>UI: 200 OK

    Note over Admin,Audit: Delete Department

    Admin->>UI: Delete department (with confirmation)
    UI->>API: DELETE /api/departments/{id}
    API->>Handler: Handle(DeleteDepartmentCommand)

    Handler->>Repo: GetDepartmentEmployeesAsync(id)
    Repo->>DB: SELECT COUNT(*)
    DB-->>Repo: Employee count

    alt Department Has Employees
        Repo-->>Handler: Count > 0
        Handler-->>UI: 400 Bad Request<br/>"Cannot delete dept with employees"
    end

    Handler->>Repo: GetSubdepartmentsAsync(id)
    Repo->>DB: SELECT COUNT(*)<br/>WHERE ParentDepartmentId = ?
    DB-->>Repo: Subdepartment count

    alt Department Has Subdepartments
        Repo-->>Handler: Count > 0
        Handler-->>UI: 400 Bad Request<br/>"Cannot delete dept with subdepartments"
    end

    Handler->>Repo: DeleteAsync(id)
    Repo->>DB: DELETE FROM Departments
    DB-->>Repo: Success

    Handler->>Audit: LogAsync("DELETE", "Department", id)
    Handler-->>API: Success
    API-->>UI: 204 No Content
    UI-->>Admin: Success + refresh tree
```

---

## 8. Transfer Employee Between Departments

```mermaid
flowchart TD
    Start([Admin initiates<br/>employee transfer])

    SelectEmployee[Select employee<br/>to transfer]
    SelectTarget[Select target<br/>department]

    ValidateTarget{Target dept<br/>exists?}
    ValidateEmployee{Employee<br/>exists?}
    ValidateSupervisor{New supervisor<br/>in target dept?}

    SelectEmployee --> ValidateEmployee
    ValidateEmployee -->|No| ErrorNotFound[Error: Employee not found]
    ValidateEmployee -->|Yes| SelectTarget

    SelectTarget --> ValidateTarget
    ValidateTarget -->|No| ErrorDeptNotFound[Error: Department not found]
    ValidateTarget -->|Yes| ValidateSupervisor

    ValidateSupervisor -->|No| PromptSupervisor[Prompt: Assign<br/>new supervisor]
    ValidateSupervisor -->|Yes| BeginTransaction

    PromptSupervisor --> BeginTransaction[Begin Database<br/>Transaction]

    BeginTransaction --> UpdateEmployee[Update Employee:<br/>- DepartmentId<br/>- SupervisorId]

    UpdateEmployee --> UpdatePermissions[Update Organizational<br/>Permissions if needed]

    UpdatePermissions --> RecalcReports[Recalculate<br/>reporting structure]

    RecalcReports --> AuditLog[Create Audit Log:<br/>"TRANSFER" action]

    AuditLog --> Notify[Send notification:<br/>- Employee<br/>- Old supervisor<br/>- New supervisor]

    Notify --> CommitTransaction[Commit Transaction]

    CommitTransaction --> RefreshUI[Refresh organization<br/>tree in UI]

    RefreshUI --> End([Transfer Complete])

    ErrorNotFound --> End
    ErrorDeptNotFound --> End

    style Start fill:#4CAF50
    style End fill:#4CAF50
    style BeginTransaction fill:#2196F3
    style CommitTransaction fill:#2196F3
    style ErrorNotFound fill:#F44336
    style ErrorDeptNotFound fill:#F44336
```

---

## 9. Pan & Zoom Implementation (Tree View)

```mermaid
sequenceDiagram
    participant User as User
    participant TreeComponent as OrgTreeChart Component
    participant PanZoom as Pan & Zoom Logic
    participant DOM as DOM Elements
    participant State as Component State

    Note over User,State: Mouse Wheel Zoom

    User->>TreeComponent: Mouse wheel event
    TreeComponent->>PanZoom: handleWheel(event)
    PanZoom->>PanZoom: Calculate zoom delta<br/>(event.deltaY * -0.001)
    PanZoom->>PanZoom: Calculate new zoom<br/>constrained (0.3 to 3.0)
    PanZoom->>PanZoom: Calculate mouse position<br/>relative to container
    PanZoom->>PanZoom: Adjust pan to keep<br/>mouse point stable
    PanZoom->>State: Update zoom and pan values
    State->>DOM: Apply CSS transform<br/>scale(zoom) translate(panX, panY)
    DOM-->>User: Smooth zoom animation

    Note over User,State: Drag to Pan

    User->>TreeComponent: Mouse down
    TreeComponent->>State: Set isDragging = true
    User->>TreeComponent: Mouse move (while dragging)
    TreeComponent->>PanZoom: handleMouseMove(event)
    PanZoom->>PanZoom: Calculate delta<br/>(current - previous position)
    PanZoom->>State: Update panX and panY
    State->>DOM: Apply CSS transform
    DOM-->>User: Pan view follows cursor
    User->>TreeComponent: Mouse up
    TreeComponent->>State: Set isDragging = false

    Note over User,State: Fit to Screen

    User->>TreeComponent: Click "Fit to Width" button
    TreeComponent->>PanZoom: fitToWidth()
    PanZoom->>DOM: Get container dimensions<br/>(wrapper.clientWidth)
    DOM-->>PanZoom: containerWidth
    PanZoom->>DOM: Get content dimensions<br/>(content.scrollWidth)
    DOM-->>PanZoom: contentWidth
    PanZoom->>PanZoom: Calculate optimal scale<br/>(containerWidth / contentWidth * 0.98)
    PanZoom->>PanZoom: Calculate center position
    PanZoom->>State: Update zoom and pan
    State->>DOM: Animate to fit
    DOM-->>User: Tree fits in viewport
```

---

## Key Features

### ✅ Implemented

1. **Unlimited Hierarchy Depth**: Supports any level of organizational structure
2. **Three Visualization Modes**: Tree (pan & zoom), Departments (cards), List (table)
3. **Department CRUD**: Create, read, update, delete departments with validation
4. **Employee Management**: Full CRUD operations for employees
5. **Organizational Permissions**: Control who can view which departments
6. **Supervisor Assignment**: Automatic handling of reporting relationships
7. **Transfer Employees**: Move employees between departments with supervisor reassignment
8. **Search & Filter**: Real-time search by name, filter by department
9. **Profile Photos**: Upload photos with fallback to initials
10. **Audit Logging**: All administrative actions tracked
11. **Department Heads**: Assign and manage department heads
12. **Bulk Operations**: Bulk assign department, transfer multiple employees

### ⚠️ Partial Implementation

- **Export to PDF/Excel**: UI buttons exist, backend returns 501 Not Implemented

### Technology Stack

**Backend:**
- GetDepartmentTree query - hierarchical structure builder
- Department repository with recursive queries
- Organizational permissions validation
- Audit logging for all CRUD operations

**Frontend:**
- PrimeVue OrganizationChart component
- Custom pan & zoom implementation
- Three view modes: Tree, Departments, List
- Employee and department modals
- Real-time search and filtering

---

*Document created: 2025-11-07*
*Version: 1.0*
*For: PortalForge v2.5 - Organizational Structure Documentation*
