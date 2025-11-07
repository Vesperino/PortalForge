# PortalForge - Request Workflow System

## System Overview

Advanced request management system with configurable templates, multi-step approval workflow, quiz functionality, and intelligent routing.

---

## 1. Request Lifecycle

```mermaid
stateDiagram-v2
    [*] --> BrowseTemplates: Employee browses<br/>available templates

    BrowseTemplates --> SelectTemplate: Select template

    SelectTemplate --> FillForm: Fill dynamic form<br/>(6 field types)

    FillForm --> SubmitRequest: Submit with priority<br/>(Standard/Urgent)

    SubmitRequest --> GenerateNumber: Generate request number<br/>(REQ-YYYY-NNNN)

    GenerateNumber --> RouteToApprover: Auto-route to first<br/>approver (Step 1)

    RouteToApprover --> PendingApproval: Status: InReview<br/>Awaiting approval

    PendingApproval --> ApproverReviews: Approver reviews

    ApproverReviews --> QuizRequired: Check if quiz required

    QuizRequired --> TakeQuiz: Quiz required
    QuizRequired --> ApprovalDecision: No quiz

    TakeQuiz --> EvaluateQuiz: Auto-grade quiz

    EvaluateQuiz --> QuizPassed: Score >= threshold
    EvaluateQuiz --> QuizFailed: Score < threshold

    QuizFailed --> PendingApproval: Retry quiz

    QuizPassed --> ApprovalDecision: Proceed to decision

    ApprovalDecision --> Approved: Approve + comment
    ApprovalDecision --> Rejected: Reject + reason

    Rejected --> NotifySubmitter: Status: Rejected
    NotifySubmitter --> [*]

    Approved --> CheckMoreSteps: Check next step

    CheckMoreSteps --> RouteToNextApprover: More steps exist
    CheckMoreSteps --> FinalApproved: All steps approved

    RouteToNextApprover --> CheckSubstitute: Check if approver<br/>on vacation

    CheckSubstitute --> AssignToSubstitute: Substitute available
    CheckSubstitute --> PendingApproval: Assign to approver

    AssignToSubstitute --> PendingApproval

    FinalApproved --> CheckVacationRequest: Is vacation request?

    CheckVacationRequest --> CreateVacation: Yes, create<br/>VacationSchedule
    CheckVacationRequest --> CheckSickLeave: No

    CreateVacation --> Completed
    CheckSickLeave --> CreateSickLeave: Yes, create<br/>SickLeave record
    CheckSickLeave --> Completed: No

    CreateSickLeave --> Completed

    Completed --> NotifyCompletion: Status: Approved<br/>Notify submitter

    NotifyCompletion --> [*]

    note right of PendingApproval
        SLA Monitoring:
        Background job checks
        deadlines hourly
    end note

    note right of FinalApproved
        Integration Points:
        - Vacation system
        - Sick leave system
        - Completion required checks
    end note
```

---

## 2. Request Template Configuration

```mermaid
graph TB
    subgraph "Admin: Create Request Template"
        Start[Admin Panel]

        BasicInfo[Template Basic Info:<br/>- Name<br/>- Description<br/>- Icon Iconify<br/>- Category]

        Visibility[Visibility Settings:<br/>- All Departments<br/>- Specific Departments<br/>- Global/Department scope]

        FormBuilder[Form Builder:<br/>6 Field Types]

        subgraph "Dynamic Form Fields"
            Text[Text Input]
            Textarea[Textarea]
            Number[Number Input]
            Select[Select Dropdown]
            Date[Date Picker]
            Checkbox[Checkbox]
        end

        ApprovalSteps[Approval Workflow<br/>Step Editor]

        subgraph "Approval Step Configuration"
            Step1[Step 1: Direct Supervisor]
            Step2[Step 2: Department Head]
            Step3[Step 3: HR Manager]
            StepN[Step N: Custom...]
        end

        ApproverTypes[6 Approver Types:<br/>- Supervisor<br/>- Role Dept<br/>- Specific User<br/>- Department Head<br/>- User Group<br/>- Submitter]

        QuizConfig[Quiz Configuration<br/>Optional per step]

        subgraph "Quiz Settings"
            Questions[Quiz Questions<br/>Multiple choice]
            Answers[Mark correct answers]
            Threshold[Pass threshold %]
        end

        SpecialFlags[Special Flags:<br/>- IsVacationRequest<br/>- IsSickLeaveRequest]

        Save[Save Template]
    end

    Start --> BasicInfo
    BasicInfo --> Visibility
    Visibility --> FormBuilder

    FormBuilder --> Text
    FormBuilder --> Textarea
    FormBuilder --> Number
    FormBuilder --> Select
    FormBuilder --> Date
    FormBuilder --> Checkbox

    Text --> ApprovalSteps
    Textarea --> ApprovalSteps
    Number --> ApprovalSteps
    Select --> ApprovalSteps
    Date --> ApprovalSteps
    Checkbox --> ApprovalSteps

    ApprovalSteps --> Step1
    ApprovalSteps --> Step2
    ApprovalSteps --> Step3
    ApprovalSteps --> StepN

    Step1 --> ApproverTypes
    Step2 --> ApproverTypes
    Step3 --> ApproverTypes

    ApproverTypes --> QuizConfig

    QuizConfig --> Questions
    Questions --> Answers
    Answers --> Threshold

    Threshold --> SpecialFlags
    SpecialFlags --> Save

    style Start fill:#4CAF50
    style FormBuilder fill:#2196F3
    style ApprovalSteps fill:#FF9800
    style QuizConfig fill:#9C27B0
    style Save fill:#4CAF50
```

---

## 3. Auto-Routing in Organizational Hierarchy

```mermaid
sequenceDiagram
    participant Employee as Employee<br/>(Submitter)
    participant System as Request System
    participant Router as Request Routing Service
    participant OrgStructure as Org Structure
    participant Approver as Approver<br/>(Auto-selected)

    Employee->>System: Submit request

    System->>Router: Route to Step 1 approver

    Router->>Router: Get Step 1 config:<br/>ApproverType = DirectSupervisor

    Router->>OrgStructure: GetEmployeeSupervisor(submitterId)

    OrgStructure-->>Router: Supervisor: John Smith

    Router->>Router: Check if supervisor on vacation

    alt Supervisor on vacation
        Router->>OrgStructure: GetSubstitute(supervisorId)
        OrgStructure-->>Router: Substitute: Jane Doe
        Router->>Approver: Assign to substitute (Jane Doe)
    else Supervisor available
        Router->>Approver: Assign to supervisor (John Smith)
    end

    Note over System,Approver: Step 1 Approved

    System->>Router: Route to Step 2 approver

    Router->>Router: Get Step 2 config:<br/>ApproverType = RoleInDepartment<br/>Role = Manager

    Router->>OrgStructure: Find Manager in submitter's department

    OrgStructure->>OrgStructure: Search hierarchy upward<br/>from submitter

    OrgStructure-->>Router: Manager: Bob Johnson

    Router->>Approver: Assign to Bob Johnson

    Note over System,Approver: Step 2 Approved

    System->>Router: Route to Step 3 approver

    Router->>Router: Get Step 3 config:<br/>ApproverType = SpecificUser<br/>UserId = hr_manager_123

    Router->>OrgStructure: GetUser(hr_manager_123)

    OrgStructure-->>Router: HR Manager: Alice Brown

    Router->>Router: Check vacation status

    Router->>Approver: Assign to Alice Brown

    Note over System,Approver: All Steps Approved

    System->>System: Mark request as Approved
    System->>Employee: Notify: Request approved
```

---

## 4. Quiz System Integration

```mermaid
sequenceDiagram
    participant Approver as Approver
    participant UI as Frontend
    participant API as Request Controller
    participant Handler as SubmitQuizAnswersHandler
    participant Quiz as Quiz Service
    participant Repo as Request Repository
    participant Notification as Notification Service

    Approver->>UI: Open pending approval
    UI->>API: GET /api/requests/{id}

    API-->>UI: Request details + Quiz questions

    UI->>UI: Display quiz required message

    Approver->>UI: Click "Take Quiz"

    UI->>UI: Show quiz modal with questions

    Note over UI: Multiple choice questions<br/>Select answers for each

    Approver->>UI: Submit quiz answers

    UI->>API: POST /api/requests/{id}/submit-quiz

    API->>Handler: Handle(SubmitQuizAnswersCommand)

    Handler->>Repo: GetRequestStepAsync(requestId, stepNumber)
    Repo-->>Handler: RequestApprovalStep with quiz

    Handler->>Quiz: GradeQuiz(answers, correctAnswers)

    Quiz->>Quiz: Count correct answers
    Quiz->>Quiz: Calculate percentage

    Quiz-->>Handler: Score: 85%

    Handler->>Handler: Compare with threshold (e.g., 80%)

    alt Quiz Passed (Score >= Threshold)
        Handler->>Repo: SaveQuizResult(Pass, score)
        Handler->>Notification: Notify: Quiz passed, proceed to approval
        Handler-->>API: Success: Quiz passed
        API-->>UI: Display success + approval button enabled
    else Quiz Failed (Score < Threshold)
        Handler->>Repo: SaveQuizResult(Fail, score)
        Handler->>Notification: Notify: Quiz failed, retry available
        Handler-->>API: Failure: Quiz failed (score too low)
        API-->>UI: Display failure + retry button
    end

    Approver->>UI: Approve/Reject request<br/>(after passing quiz)

    UI->>API: POST /api/requests/{id}/approve

    API-->>UI: Approval recorded
    UI->>UI: Move to next step<br/>or mark as approved
```

---

## 5. SLA Monitoring & Escalation

```mermaid
flowchart TD
    Start([Background Job:<br/>Check SLA Deadlines<br/>Hourly])

    GetPendingRequests[Get all requests<br/>in InReview status]

    GetPendingRequests --> LoopRequests[For each request]

    LoopRequests --> GetCurrentStep[Get current<br/>approval step]

    GetCurrentStep --> CheckDeadline{Has deadline<br/>& overdue?}

    CheckDeadline -->|No| NextRequest[Next request]
    CheckDeadline -->|Yes| CalculateOverdue[Calculate days overdue]

    CalculateOverdue --> CheckAlreadyNotified{Reminder already<br/>sent today?}

    CheckAlreadyNotified -->|Yes| NextRequest
    CheckAlreadyNotified -->|No| SendReminder[Send SLA reminder<br/>to approver]

    SendReminder --> CreateNotification[Create notification:<br/>"Approval overdue"]

    CreateNotification --> SendEmail[Send email reminder]

    SendEmail --> LogReminder[Log reminder sent<br/>(timestamp)]

    LogReminder --> CheckEscalation{Days overdue<br/>> 3?}

    CheckEscalation -->|No| NextRequest
    CheckEscalation -->|Yes| EscalateToSupervisor[Escalate to<br/>approver's supervisor]

    EscalateToSupervisor --> NotifySupervisor[Notify supervisor<br/>of overdue approval]

    NotifySupervisor --> NextRequest

    NextRequest --> MoreRequests{More requests?}

    MoreRequests -->|Yes| LoopRequests
    MoreRequests -->|No| End([Job Complete])

    style Start fill:#FF8A65
    style SendReminder fill:#FF9800
    style EscalateToSupervisor fill:#E91E63
    style End fill:#4CAF50
```

---

## 6. Request UI - Four Tabs

```mermaid
graph TB
    RequestHub[Request Hub Page<br/>/dashboard/requests]

    subgraph "Tab 1: New Request"
        BrowseTemplates[Browse Templates<br/>by Category]
        TemplateCards[Template Cards<br/>with Icons & Descriptions]
        SelectTemplate[Click template]
        NavigateToForm[Navigate to<br/>submit form]
    end

    subgraph "Tab 2: My Requests"
        MyRequestsList[List of submitted<br/>requests]
        StatusFilter[Filter by status:<br/>InReview, Approved,<br/>Rejected]
        SearchRequests[Search by<br/>request number]
        ViewDetails[View request details]
        Timeline[Visual timeline<br/>of approvals]
    end

    subgraph "Tab 3: To Approve"
        PendingApprovals[Requests pending<br/>my approval]
        QuizBadge[Badge: Quiz required]
        PriorityBadge[Badge: Urgent]
        ApproveRejectButtons[Approve/Reject<br/>buttons]
        CommentModal[Add comment modal]
    end

    subgraph "Tab 4: Approved by Me"
        ApprovalHistory[History of<br/>my approvals]
        FilterByAction[Filter:<br/>Approved/Rejected]
        ViewPastDecisions[View past decisions<br/>& comments]
    end

    RequestHub --> BrowseTemplates
    RequestHub --> MyRequestsList
    RequestHub --> PendingApprovals
    RequestHub --> ApprovalHistory

    BrowseTemplates --> TemplateCards
    TemplateCards --> SelectTemplate
    SelectTemplate --> NavigateToForm

    MyRequestsList --> StatusFilter
    MyRequestsList --> SearchRequests
    SearchRequests --> ViewDetails
    ViewDetails --> Timeline

    PendingApprovals --> QuizBadge
    PendingApprovals --> PriorityBadge
    PriorityBadge --> ApproveRejectButtons
    QuizBadge --> TakeQuiz[Take Quiz first]
    TakeQuiz --> ApproveRejectButtons
    ApproveRejectButtons --> CommentModal

    ApprovalHistory --> FilterByAction
    FilterByAction --> ViewPastDecisions

    style RequestHub fill:#4CAF50
    style BrowseTemplates fill:#2196F3
    style MyRequestsList fill:#FF9800
    style PendingApprovals fill:#9C27B0
    style ApprovalHistory fill:#607D8B
```

---

## 7. Data Model

```mermaid
erDiagram
    RequestTemplate ||--o{ RequestTemplateField : has
    RequestTemplate ||--o{ RequestApprovalStepTemplate : has
    RequestApprovalStepTemplate ||--o{ QuizQuestion : has
    QuizQuestion ||--o{ QuizAnswer : has

    Request ||--|| RequestTemplate : based_on
    Request ||--o{ RequestApprovalStep : has
    Request ||--o{ RequestComment : has
    Request ||--o{ RequestEditHistory : tracks

    RequestApprovalStep ||--o{ QuizQuestion : may_have
    RequestApprovalStep ||--|| User : assigned_to

    Request ||--|| User : submitted_by
    RequestComment ||--|| User : written_by

    RequestTemplate {
        int TemplateId PK
        string Name
        string Description
        string IconName
        string Category
        bool IsActive
        bool IsVacationRequest
        bool IsSickLeaveRequest
        int EstimatedProcessingDays
    }

    RequestTemplateField {
        int FieldId PK
        int TemplateId FK
        string FieldName
        string FieldType
        bool IsRequired
        string Options
        int DisplayOrder
    }

    RequestApprovalStepTemplate {
        int StepTemplateId PK
        int TemplateId FK
        int StepNumber
        string ApproverType
        string ApproverIdentifier
        bool RequiresQuiz
        int QuizPassingScore
    }

    QuizQuestion {
        int QuestionId PK
        int StepTemplateId FK
        string QuestionText
        string AnswerOptions
        string CorrectAnswer
    }

    Request {
        int RequestId PK
        string RequestNumber
        string SubmitterId FK
        int TemplateId FK
        string FormData
        string Status
        string Priority
        datetime SubmittedAt
    }

    RequestApprovalStep {
        int StepId PK
        int RequestId FK
        int StepNumber
        string ApproverId FK
        string Status
        string Comment
        int QuizScore
        datetime ApprovedAt
    }

    RequestComment {
        int CommentId PK
        int RequestId FK
        string UserId FK
        string CommentText
        string AttachmentUrl
        datetime CreatedAt
    }
```

---

## Key Features

### ✅ Fully Implemented (100%)

1. **Configurable Templates**: Admin panel for creating templates with drag-and-drop field builder
2. **6 Field Types**: Text, Textarea, Number, Select, Date, Checkbox
3. **Multi-Step Approval**: Unlimited approval steps with sequential workflow
4. **6 Approver Types**: Supervisor, Role, Specific User, Department, User Group, Submitter
5. **Quiz System**: Multiple-choice quizzes with auto-grading and pass thresholds
6. **Auto-Routing**: Intelligent routing based on organizational hierarchy
7. **Vacation Substitution**: Automatic routing to substitutes when approver on vacation
8. **Vacation Integration**: Auto-create VacationSchedule from approved vacation requests
9. **Sick Leave L4**: Auto-approval flow with SickLeave record creation
10. **Comments & Attachments**: Full commenting system with file uploads
11. **Edit History**: Complete audit trail of all request changes
12. **SLA Monitoring**: Background job checking deadlines with reminders
13. **Request Hub**: 4 tabs (New Request, My Requests, To Approve, Approved by Me)
14. **Visual Timeline**: Step-by-step approval progress visualization
15. **Priority Levels**: Standard/Urgent with visual badges
16. **Status Tracking**: Draft, InReview, Approved, Rejected, AwaitingSurvey
17. **Department Scope**: Templates visible to all or specific departments

### Technology

**Backend:**
- Request routing service with hierarchy traversal
- Quiz grading service
- SLA deadline checker (background job)
- Vacation/sick leave integration
- FluentValidation for all commands

**Frontend:**
- Dynamic form builder
- Approval step editor with drag-and-drop
- Quiz modal with real-time grading
- Request timeline visualization
- Comment system with attachments

---

*Document created: 2025-11-07*
*Version: 1.0*
*For: PortalForge v2.5 - Request Workflow Documentation*
