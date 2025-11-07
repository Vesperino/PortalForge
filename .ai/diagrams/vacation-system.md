# PortalForge - Vacation Management System

## System Overview

Comprehensive vacation management system with calendar views, conflict detection, substitutions, and automated workflows.

---

## 1. Vacation Request & Approval Flow

```mermaid
stateDiagram-v2
    [*] --> SubmitRequest: Employee submits<br/>vacation request

    SubmitRequest --> ValidateDates: Validate dates<br/>(business days only)
    ValidateDates --> CheckAvailability: Check vacation<br/>days available
    CheckAvailability --> AssignSubstitute: Employee assigns<br/>substitute

    AssignSubstitute --> SupervisorApproval: Pending<br/>Supervisor Approval

    SupervisorApproval --> CheckSubstituteVacation: Supervisor reviews

    CheckSubstituteVacation --> Approved: Approve
    CheckSubstituteVacation --> Rejected: Reject

    Rejected --> [*]: Notify employee

    Approved --> CheckConflicts: Check team<br/>conflicts (>30%)

    CheckConflicts --> WarningIssued: Conflict detected<br/>(30-50%)
    CheckConflicts --> CriticalAlert: Critical conflict<br/>(>50%)
    CheckConflicts --> Scheduled: No conflicts

    WarningIssued --> Scheduled: Continue with warning
    CriticalAlert --> Scheduled: Continue with alert

    Scheduled --> NotifyEmployee: Send confirmation<br/>email

    NotifyEmployee --> NotifySubstitute: Notify substitute<br/>(7 days before)

    NotifySubstitute --> Active: Vacation starts<br/>(auto-update status)

    Active --> Completed: Vacation ends<br/>(auto-update status)

    Completed --> [*]

    note right of Scheduled
        Status: Scheduled
        Background job checks
        daily for transitions
    end note

    note right of Active
        Status: Active
        Substitute activated
        Approval routing updated
    end note
```

---

## 2. Team Calendar with Conflict Detection

```mermaid
sequenceDiagram
    participant Employee as Employee
    participant UI as Frontend Calendar
    participant API as Vacation Controller
    participant Handler as GetTeamCalendarHandler
    participant ConflictService as Conflict Detection
    participant Repo as Vacation Repository
    participant DB as Database

    Employee->>UI: View Team Calendar
    UI->>API: GET /api/vacation-schedules/team<br/>?departmentId=5&year=2025&month=11

    API->>Handler: Handle(GetTeamCalendarQuery)

    Handler->>Repo: GetDepartmentEmployeesAsync(deptId)
    Repo->>DB: SELECT * FROM Employees<br/>WHERE DepartmentId = ?
    DB-->>Repo: List of employees
    Repo-->>Handler: Team members

    Handler->>Repo: GetVacationsByDepartmentAsync(deptId, dateRange)
    Repo->>DB: SELECT * FROM VacationSchedules<br/>WHERE EmployeeId IN (...)<br/>AND Status IN ('Scheduled', 'Active')
    DB-->>Repo: All team vacations
    Repo-->>Handler: Vacation schedules

    Handler->>ConflictService: DetectConflicts(vacations, teamSize)

    loop For each date in month
        ConflictService->>ConflictService: Count employees on vacation
        ConflictService->>ConflictService: Calculate percentage (count / teamSize)

        alt Percentage > 50%
            ConflictService->>ConflictService: Mark as CRITICAL conflict
        else Percentage > 30%
            ConflictService->>ConflictService: Mark as WARNING
        else
            ConflictService->>ConflictService: No conflict
        end
    end

    ConflictService-->>Handler: Conflict analysis

    Handler-->>API: TeamCalendarDto with conflicts
    API-->>UI: JSON response

    UI->>UI: Render calendar:<br/>- Timeline view (Gantt)<br/>- Grid view (Calendar)

    UI->>UI: Display conflict alerts:<br/>- Red badge for >50%<br/>- Yellow badge for 30-50%

    UI-->>Employee: Visual calendar with warnings
```

---

## 3. Background Jobs Automation

```mermaid
graph TB
    subgraph "Hangfire Background Jobs"
        Scheduler[Hangfire Scheduler]

        Job1[Update Vacation Statuses<br/>Every 6 hours]
        Job2[Send Vacation Reminders<br/>Daily at 8:00 AM]
        Job3[Update Annual Allowances<br/>January 1st yearly]
        Job4[Expire Carried-Over Days<br/>March 31st yearly]
        Job5[Carried-Over Reminders<br/>Weekly in March]
    end

    subgraph "Job 1: Update Statuses"
        CheckScheduled[Check Scheduled<br/>vacations]
        CheckActive[Check Active<br/>vacations]
        UpdateToActive[Start date reached<br/>→ Active]
        UpdateToCompleted[End date passed<br/>→ Completed]
    end

    subgraph "Job 2: Send Reminders"
        Check7Days[Vacation in 7 days<br/>Send reminder]
        Check1Day[Vacation tomorrow<br/>Send reminder]
        CheckStarting[Vacation starts today<br/>Notify substitute]
        CheckEnding[Vacation ends today<br/>Welcome back]
    end

    subgraph "Job 3: Annual Allowances"
        ResetAllowances[Reset annual<br/>vacation days]
        CalculateCarriedOver[Calculate carried-over<br/>from previous year]
        SetExpiryDate[Set expiry date<br/>(March 31st)]
    end

    subgraph "Job 4: Expire Carried-Over"
        FindExpired[Find carried-over days<br/>past March 31st]
        DeductDays[Deduct expired days]
        NotifyUsers[Notify affected users]
    end

    subgraph "Job 5: Expiry Warnings"
        FindExpiring[Find carried-over days<br/>expiring soon]
        SendWarnings[Send weekly reminders<br/>in March]
    end

    Scheduler --> Job1
    Scheduler --> Job2
    Scheduler --> Job3
    Scheduler --> Job4
    Scheduler --> Job5

    Job1 --> CheckScheduled
    Job1 --> CheckActive
    CheckScheduled --> UpdateToActive
    CheckActive --> UpdateToCompleted

    Job2 --> Check7Days
    Job2 --> Check1Day
    Job2 --> CheckStarting
    Job2 --> CheckEnding

    Job3 --> ResetAllowances
    Job3 --> CalculateCarriedOver
    Job3 --> SetExpiryDate

    Job4 --> FindExpired
    Job4 --> DeductDays
    Job4 --> NotifyUsers

    Job5 --> FindExpiring
    Job5 --> SendWarnings

    style Scheduler fill:#FF8A65
    style Job1 fill:#81C784
    style Job2 fill:#64B5F6
    style Job3 fill:#FFD54F
    style Job4 fill:#E57373
    style Job5 fill:#BA68C8
```

---

## 4. Substitute Management Flow

```mermaid
sequenceDiagram
    participant Employee as Employee A<br/>(on vacation)
    participant System as Vacation System
    participant Substitute as Employee B<br/>(substitute)
    participant Request as Request System
    participant Approver as Actual Approver

    Note over Employee,Approver: Vacation Scheduled (7 days before)

    System->>Substitute: Email: You're substitute for Employee A<br/>Vacation: Dec 10-15

    Note over Employee,Approver: Vacation Starts (Dec 10)

    System->>System: Update EmployeeA status: OnVacation
    System->>System: Activate substitute: EmployeeB

    Note over Employee,Approver: Request Submitted (Dec 12)

    Request->>Request: Check approver: EmployeeA

    alt EmployeeA is on vacation
        Request->>Request: Get substitute: EmployeeB
        Request->>Substitute: Assign approval to substitute
        Substitute->>Substitute: Receives notification:<br/>"Approval needed (as substitute)"
    end

    Substitute->>Request: Approve/Reject request

    Request->>Approver: Route to next approver

    Note over Employee,Approver: Vacation Ends (Dec 15)

    System->>System: Update EmployeeA status: Active
    System->>System: Deactivate substitute role

    System->>Employee: Email: Welcome back!
    System->>Substitute: Email: Substitute duty ended
```

---

## 5. Vacation Allowance Tracking

```mermaid
graph TB
    subgraph "Vacation Days Calculation"
        Annual[Annual Vacation Days<br/>Default: 26 days]
        CarriedOver[Carried-Over from Previous Year<br/>Max: 26 days<br/>Expires: March 31st]
        OnDemand[On-Demand Vacation<br/>Max: 4 days per year]
        Circumstantial[Circumstantial Leave<br/>Special cases]
    end

    subgraph "Days Usage"
        TotalAvailable[Total Available Days]
        DaysUsed[Days Used<br/>(Completed vacations)]
        DaysScheduled[Days Scheduled<br/>(Approved, not started)]
        DaysRemaining[Days Remaining]
    end

    Annual --> TotalAvailable
    CarriedOver --> TotalAvailable
    OnDemand --> TotalAvailable
    Circumstantial --> TotalAvailable

    TotalAvailable --> DaysRemaining
    DaysUsed --> DaysRemaining
    DaysScheduled --> DaysRemaining

    DaysRemaining --> DisplayToUser[Display in UI:<br/>Progress bars<br/>Statistics]

    subgraph "Annual Reset (January 1st)"
        ResetAnnual[Reset Annual to 26]
        MoveToCarriedOver[Unused days → Carried-Over<br/>Max 26]
        ResetOnDemand[Reset On-Demand to 4]
    end

    DaysRemaining --> ResetAnnual
    ResetAnnual --> MoveToCarriedOver
    MoveToCarriedOver --> ResetOnDemand

    subgraph "Expiry (March 31st)"
        CheckCarriedOver[Check Carried-Over Days]
        ExpireDays[Expire days past March 31st]
        NotifyExpiry[Notify users of expiry]
    end

    MoveToCarriedOver --> CheckCarriedOver
    CheckCarriedOver --> ExpireDays
    ExpireDays --> NotifyExpiry

    style Annual fill:#4CAF50
    style CarriedOver fill:#FF9800
    style OnDemand fill:#2196F3
    style Circumstantial fill:#9C27B0
    style DaysRemaining fill:#E91E63
```

---

## 6. Sick Leave (L4) Auto-Approval

```mermaid
flowchart TD
    Start([Employee submits<br/>Sick Leave request])

    CheckTemplate{Request template<br/>has IsSickLeaveRequest<br/>flag?}

    CheckTemplate -->|No| NormalFlow[Normal approval<br/>workflow]
    CheckTemplate -->|Yes| AutoApprove[Auto-approve<br/>all steps]

    AutoApprove --> ExtractDates[Extract dates<br/>from form data]

    ExtractDates --> CreateSickLeave[Create SickLeave<br/>record in DB]

    CreateSickLeave --> CalculateDays[Calculate<br/>business days]

    CalculateDays --> CheckDuration{Duration<br/>> 33 days?}

    CheckDuration -->|No| NotifySupervisor[Notify supervisor]
    CheckDuration -->|Yes| ZUSReminder[Send ZUS documentation<br/>reminder]

    ZUSReminder --> NotifySupervisor

    NotifySupervisor --> UpdateVacation[Update vacation days<br/>if needed]

    UpdateVacation --> Complete([Sick Leave<br/>Approved & Tracked])

    NormalFlow --> NormalComplete([Regular approval<br/>process])

    style CheckTemplate fill:#FF9800
    style AutoApprove fill:#4CAF50
    style ZUSReminder fill:#E91E63
    style Complete fill:#4CAF50
```

---

## 7. Team Calendar UI Views

### Timeline View (Gantt)
```mermaid
gantt
    title Team Vacation Schedule - November 2025
    dateFormat YYYY-MM-DD
    section John Smith
    Vacation :done, 2025-11-03, 2025-11-07
    section Jane Doe
    Vacation :active, 2025-11-10, 2025-11-17
    section Bob Johnson
    Vacation :crit, 2025-11-12, 2025-11-14
    section Alice Brown
    Vacation : 2025-11-20, 2025-11-27
```

### Conflict Detection Logic
- **Normal**: < 30% of team on vacation (green)
- **Warning**: 30-50% of team on vacation (yellow)
- **Critical**: > 50% of team on vacation (red)

---

## Key Features

### ✅ Implemented (95%)

1. **Vacation Request Submission**: Full form with date picker, substitute selection
2. **Approval Workflow**: Supervisor approval with validation
3. **Team Calendar**: Two views (Timeline Gantt, Calendar Grid)
4. **Conflict Detection**: Real-time alerts for 30%/50% thresholds
5. **Substitute System**: Automatic assignment and approval routing
6. **Background Jobs**: 5 automated tasks (status updates, reminders, annual reset, expiry)
7. **Email Notifications**: 7 days before, 1 day before, start, end
8. **Vacation Allowances**: Annual, carried-over, on-demand, circumstantial tracking
9. **Sick Leave L4**: Auto-approval with ZUS reminders for >33 days
10. **Statistics Dashboard**: Personal and team vacation usage

### ⚠️ Partial (5%)

- **PDF/Excel Export**: Endpoints exist, return 501 Not Implemented

---

*Document created: 2025-11-07*
*Version: 1.0*
*For: PortalForge v2.5 - Vacation System Documentation*
