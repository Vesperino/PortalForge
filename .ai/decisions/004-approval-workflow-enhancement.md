# Approval Workflow Enhancement - Design Document

**Date:** 2025-10-30
**Status:** ✅ Implemented (Sprint 1 & 2 Complete)
**Implementation Date:** 2025-10-30
**Author:** AI Assistant

---

## ✅ IMPLEMENTATION STATUS

### Sprint 1: Core Flexible Approver Assignment - ✅ COMPLETE
- ✅ Created `ApproverType` enum (Role, SpecificUser, UserGroup, Submitter)
- ✅ Enhanced `RequestApprovalStepTemplate` entity with new fields
- ✅ Created and applied database migration `AddApprovalWorkflowEnhancements`
- ✅ Updated `SubmitRequestCommandHandler` with `ResolveApproversAsync()` method
- ✅ Frontend types updated with `ApproverType` and enhanced `RequestApprovalStepTemplate`

### Sprint 2: Notification System - ✅ COMPLETE
- ✅ Created `NotificationType` enum (7 types)
- ✅ Created `Notification` entity with EF Core configuration
- ✅ Created `INotificationRepository` and `NotificationRepository`
- ✅ Created `INotificationService` and `NotificationService`
- ✅ Updated `SubmitRequestCommandHandler` to send notifications to first approvers
- ✅ Updated `ApproveRequestStepCommandHandler` to send notifications
- ✅ Created `RejectRequestStepCommand` and handler with notifications
- ✅ Created notification queries: `GetUserNotificationsQuery`, `GetUnreadCountQuery`
- ✅ Created notification commands: `MarkAsReadCommand`, `MarkAllAsReadCommand`
- ✅ Created `NotificationsController` with full API endpoints
- ✅ Created `GetPendingApprovalsQuery` and handler
- ✅ Updated `RequestsController` with `/pending-approvals` and `/reject` endpoints
- ✅ Frontend: Created `useNotificationsStore` with polling
- ✅ Frontend: Created `NotificationBell.vue` component
- ✅ Frontend: Integrated NotificationBell into `layouts/default.vue`
- ✅ Frontend: Created `/dashboard/requests/pending-approvals` page
- ✅ Frontend: Updated `useRequestsApi` composable with all new methods

### Sprint 3: UI for Approver Type Selection - ✅ COMPLETE
- ✅ Created `useUsersApi` composable for fetching users
- ✅ Extended `useRoleGroupApi` with `getAllRoleGroups()` method
- ✅ Created `ApprovalStepEditor.vue` component with:
  - Approver type selection (Role, SpecificUser, UserGroup, Submitter)
  - Conditional UI based on selected type
  - User search with autocomplete
  - Group selection dropdown
- ✅ Updated template creation page to use new component
- ✅ Loading states and error handling

### Sprint 4: Advanced Features - 🔜 PLANNED
- ⏳ Update edit template form with same functionality
- ⏳ Delegation system
- ⏳ Escalation with SLA tracking
- ⏳ Parallel approval (all/any from group)
- ⏳ Conditional routing based on form data

### Sprint 5: Best Practices & Polish - 🔜 PLANNED
- ⏳ Audit trail for all approval actions
- ⏳ Email notifications (in addition to in-app)
- ⏳ Mobile-responsive approval interface
- ⏳ Analytics dashboard for approval metrics

---

## Executive Summary

This document outlines the enhancement of the PortalForge approval workflow system to support:
1. Flexible approver assignment (Role, Specific User, User Group)
2. In-app notification system for approvers
3. "Pending Approvals" view for approvers
4. Best practices from enterprise workflow systems

## Current State Analysis

### Existing Entities

**RequestApprovalStepTemplate** (Template level):
```csharp
public class RequestApprovalStepTemplate
{
    public Guid Id { get; set; }
    public Guid RequestTemplateId { get; set; }
    public int StepOrder { get; set; }
    public DepartmentRole ApproverRole { get; set; }  // ❌ Only supports roles
    public bool RequiresQuiz { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**RequestApprovalStep** (Instance level):
```csharp
public class RequestApprovalStep
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public int StepOrder { get; set; }
    public Guid ApproverId { get; set; }  // ✅ Specific user
    public User Approver { get; set; }
    public ApprovalStepStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? Comment { get; set; }
    public bool RequiresQuiz { get; set; }
    public int? QuizScore { get; set; }
    public bool? QuizPassed { get; set; }
}
```

### Current Workflow Logic

**Template Creation** (`CreateRequestTemplateCommandHandler`):
- Admin defines approval steps with `ApproverRole` (Manager, Director)
- No support for specific users or groups

**Request Submission** (`SubmitRequestCommandHandler`):
```csharp
foreach (var stepTemplate in orderedSteps)
{
    User? approver = null;
    
    if (stepTemplate.ApproverRole == DepartmentRole.Manager)
        approver = submitter.Supervisor;  // Direct supervisor
    else if (stepTemplate.ApproverRole == DepartmentRole.Director)
        approver = submitter.Supervisor?.Supervisor;  // Supervisor's supervisor
    
    if (approver != null)
    {
        var approvalStep = new RequestApprovalStep
        {
            ApproverId = approver.Id,
            Status = stepTemplate.StepOrder == 1 
                ? ApprovalStepStatus.InReview 
                : ApprovalStepStatus.Pending
        };
        request.ApprovalSteps.Add(approvalStep);
    }
}
```

**Approval Processing** (`ApproveRequestStepCommandHandler`):
- Validates approver authorization
- Checks quiz requirements
- Approves current step
- Moves to next step or completes request
- ❌ No notifications sent

### Limitations

1. **Inflexible Approver Assignment**: Only supports hierarchical roles (Manager, Director)
2. **No Notifications**: Approvers don't know when requests need their attention
3. **No Approver Dashboard**: No dedicated view for pending approvals
4. **No Delegation**: Approvers can't delegate to others
5. **No Escalation**: No automatic escalation for overdue approvals
6. **Sequential Only**: No parallel approval support
7. **No Conditional Routing**: Can't route based on form data

## Proposed Solution

### Phase 1: Flexible Approver Assignment (PRIORITY)

#### 1.1 New Enum: ApproverType

```csharp
namespace PortalForge.Domain.Enums;

public enum ApproverType
{
    Role,           // Hierarchical role (Manager, Director)
    SpecificUser,   // Specific user by ID
    UserGroup,      // Any user from a RoleGroup
    Submitter       // The person who submitted the request (for self-approval scenarios)
}
```

#### 1.2 Enhanced RequestApprovalStepTemplate

```csharp
public class RequestApprovalStepTemplate
{
    public Guid Id { get; set; }
    public Guid RequestTemplateId { get; set; }
    public RequestTemplate RequestTemplate { get; set; } = null!;
    
    public int StepOrder { get; set; }
    
    // NEW: Flexible approver assignment
    public ApproverType ApproverType { get; set; }
    
    // For ApproverType.Role
    public DepartmentRole? ApproverRole { get; set; }
    
    // For ApproverType.SpecificUser
    public Guid? SpecificUserId { get; set; }
    public User? SpecificUser { get; set; }
    
    // For ApproverType.UserGroup
    public Guid? ApproverGroupId { get; set; }
    public RoleGroup? ApproverGroup { get; set; }
    
    public bool RequiresQuiz { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
```

#### 1.3 Enhanced Approver Resolution Logic

```csharp
// In SubmitRequestCommandHandler
private async Task<List<User>> ResolveApproversAsync(
    RequestApprovalStepTemplate stepTemplate, 
    User submitter)
{
    var approvers = new List<User>();
    
    switch (stepTemplate.ApproverType)
    {
        case ApproverType.Role:
            var roleApprover = await ResolveRoleApproverAsync(
                stepTemplate.ApproverRole!.Value, 
                submitter
            );
            if (roleApprover != null) approvers.Add(roleApprover);
            break;
            
        case ApproverType.SpecificUser:
            var specificUser = await _unitOfWork.UserRepository
                .GetByIdAsync(stepTemplate.SpecificUserId!.Value);
            if (specificUser != null) approvers.Add(specificUser);
            break;
            
        case ApproverType.UserGroup:
            var groupUsers = await _unitOfWork.UserRepository
                .GetUsersByRoleGroupIdAsync(stepTemplate.ApproverGroupId!.Value);
            approvers.AddRange(groupUsers);
            break;
            
        case ApproverType.Submitter:
            approvers.Add(submitter);
            break;
    }
    
    return approvers;
}

private async Task<User?> ResolveRoleApproverAsync(
    DepartmentRole role, 
    User submitter)
{
    return role switch
    {
        DepartmentRole.Manager => submitter.Supervisor,
        DepartmentRole.Director => submitter.Supervisor?.Supervisor,
        _ => null
    };
}
```

### Phase 2: Notification System

#### 2.1 New Entity: Notification

```csharp
namespace PortalForge.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    // Link to related entity
    public string? RelatedEntityType { get; set; }  // "Request", "News", etc.
    public string? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
```

#### 2.2 New Enum: NotificationType

```csharp
namespace PortalForge.Domain.Enums;

public enum NotificationType
{
    RequestPendingApproval,     // Request needs your approval
    RequestApproved,            // Your request was approved
    RequestRejected,            // Your request was rejected
    RequestCompleted,           // Your request is complete
    RequestCommented,           // Someone commented on your request
    System,                     // System notification
    Announcement                // Company announcement
}
```

#### 2.3 Notification Service

```csharp
public interface INotificationService
{
    Task CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null
    );

    Task NotifyApproverAsync(Guid approverId, Request request);
    Task NotifySubmitterAsync(Request request, string message);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}
```

#### 2.4 Integration Points

**When request is submitted:**
```csharp
// In SubmitRequestCommandHandler, after creating approval steps
var firstStepApprovers = request.ApprovalSteps
    .Where(s => s.StepOrder == 1)
    .Select(s => s.ApproverId)
    .Distinct();

foreach (var approverId in firstStepApprovers)
{
    await _notificationService.NotifyApproverAsync(approverId, request);
}
```

**When request is approved:**
```csharp
// In ApproveRequestStepCommandHandler
if (nextStep != null)
{
    // Notify next approver
    await _notificationService.NotifyApproverAsync(nextStep.ApproverId, request);
}
else
{
    // Notify submitter of completion
    await _notificationService.NotifySubmitterAsync(
        request,
        "Your request has been approved and completed"
    );
}
```

**When request is rejected:**
```csharp
// In RejectRequestStepCommandHandler
await _notificationService.NotifySubmitterAsync(
    request,
    $"Your request has been rejected. Reason: {command.Comment}"
);
```

### Phase 3: Pending Approvals View

#### 3.1 New Query: GetPendingApprovalsQuery

```csharp
public class GetPendingApprovalsQuery : IRequest<List<RequestDto>>
{
    public Guid ApproverId { get; set; }
    public string? Status { get; set; }  // "InReview", "RequiresSurvey", null = all
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetPendingApprovalsQueryHandler
    : IRequestHandler<GetPendingApprovalsQuery, List<RequestDto>>
{
    public async Task<List<RequestDto>> Handle(
        GetPendingApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Requests
            .Include(r => r.RequestTemplate)
            .Include(r => r.SubmittedBy)
            .Include(r => r.ApprovalSteps)
                .ThenInclude(s => s.Approver)
            .Where(r => r.ApprovalSteps.Any(s =>
                s.ApproverId == request.ApproverId &&
                s.Status == ApprovalStepStatus.InReview
            ));

        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = Enum.Parse<ApprovalStepStatus>(request.Status);
            query = query.Where(r => r.ApprovalSteps.Any(s =>
                s.ApproverId == request.ApproverId &&
                s.Status == status
            ));
        }

        var requests = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToDto).ToList();
    }
}
```

#### 3.2 Frontend: Pending Approvals Page

**Route:** `/dashboard/requests/pending-approvals`

**Features:**
- List of requests awaiting approval
- Filter by status (InReview, RequiresSurvey)
- Quick actions: Approve, Reject, View Details
- Badge showing count of pending approvals

#### 3.3 Frontend: Notification Bell Component

```vue
<!-- components/NotificationBell.vue -->
<template>
  <div class="relative">
    <button
      @click="toggleDropdown"
      class="header-icon-btn"
      aria-label="Notifications"
    >
      <Bell class="w-6 h-6" />
      <span v-if="unreadCount > 0" class="notification-badge">
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <transition name="dropdown">
      <div v-if="isOpen" class="notification-dropdown">
        <div class="notification-header">
          <h3>Notifications</h3>
          <button @click="markAllAsRead">Mark all as read</button>
        </div>

        <div class="notification-list">
          <div
            v-for="notification in notifications"
            :key="notification.id"
            class="notification-item"
            :class="{ 'unread': !notification.isRead }"
            @click="handleNotificationClick(notification)"
          >
            <div class="notification-icon">
              <component :is="getNotificationIcon(notification.type)" />
            </div>
            <div class="notification-content">
              <p class="notification-title">{{ notification.title }}</p>
              <p class="notification-message">{{ notification.message }}</p>
              <p class="notification-time">{{ formatTime(notification.createdAt) }}</p>
            </div>
          </div>
        </div>

        <div class="notification-footer">
          <NuxtLink to="/dashboard/notifications">View all</NuxtLink>
        </div>
      </div>
    </transition>
  </div>
</template>
```

### Phase 4: Best Practices from Enterprise Systems

#### 4.1 Delegation

**Use Case:** Manager is on vacation, needs to delegate approvals to another manager.

**Implementation:**
```csharp
public class ApprovalDelegation
{
    public Guid Id { get; set; }
    public Guid FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    public Guid ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Reason { get; set; }
}
```

**Logic:**
```csharp
// When resolving approver, check for active delegations
var delegation = await _context.ApprovalDelegations
    .FirstOrDefaultAsync(d =>
        d.FromUserId == originalApproverId &&
        d.IsActive &&
        d.StartDate <= DateTime.UtcNow &&
        d.EndDate >= DateTime.UtcNow
    );

var effectiveApproverId = delegation?.ToUserId ?? originalApproverId;
```

#### 4.2 Escalation

**Use Case:** Request not approved within SLA, escalate to higher authority.

**Implementation:**
```csharp
public class RequestApprovalStep
{
    // ... existing properties

    public int? SlaHours { get; set; }  // From template
    public DateTime? EscalationDate { get; set; }
    public Guid? EscalatedToUserId { get; set; }
    public User? EscalatedToUser { get; set; }
    public bool IsEscalated { get; set; } = false;
}
```

**Background Job:**
```csharp
public class EscalationBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var overdueSteps = await _context.RequestApprovalSteps
                .Where(s =>
                    s.Status == ApprovalStepStatus.InReview &&
                    !s.IsEscalated &&
                    s.SlaHours.HasValue &&
                    s.StartedAt.HasValue &&
                    s.StartedAt.Value.AddHours(s.SlaHours.Value) < DateTime.UtcNow
                )
                .ToListAsync(stoppingToken);

            foreach (var step in overdueSteps)
            {
                await EscalateStepAsync(step);
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
```

#### 4.3 Parallel Approval

**Use Case:** Multiple approvers must approve simultaneously (e.g., both Finance and IT must approve).

**Implementation:**
```csharp
public class RequestApprovalStepTemplate
{
    // ... existing properties

    public ApprovalMode ApprovalMode { get; set; } = ApprovalMode.Sequential;
    public int? RequiredApprovals { get; set; }  // For parallel mode
}

public enum ApprovalMode
{
    Sequential,  // One after another (current behavior)
    Parallel,    // All must approve
    AnyOne       // Any one approver is sufficient
}
```

**Logic:**
```csharp
// When creating approval steps for parallel mode
if (stepTemplate.ApprovalMode == ApprovalMode.Parallel)
{
    foreach (var approver in approvers)
    {
        request.ApprovalSteps.Add(new RequestApprovalStep
        {
            StepOrder = stepTemplate.StepOrder,
            ApproverId = approver.Id,
            Status = ApprovalStepStatus.InReview,  // All start immediately
            StartedAt = DateTime.UtcNow
        });
    }
}

// When approving
if (stepTemplate.ApprovalMode == ApprovalMode.Parallel)
{
    var parallelSteps = request.ApprovalSteps
        .Where(s => s.StepOrder == currentStep.StepOrder)
        .ToList();

    var approvedCount = parallelSteps.Count(s => s.Status == ApprovalStepStatus.Approved);
    var requiredCount = stepTemplate.RequiredApprovals ?? parallelSteps.Count;

    if (approvedCount >= requiredCount)
    {
        // Move to next step
        MoveToNextStep(request);
    }
}
```

#### 4.4 Conditional Routing

**Use Case:** Route to different approvers based on form data (e.g., amount > $10,000 requires CFO approval).

**Implementation:**
```csharp
public class RequestApprovalStepTemplate
{
    // ... existing properties

    public string? ConditionExpression { get; set; }  // JSON or expression
}

// Example condition:
{
  "field": "amount",
  "operator": "greaterThan",
  "value": 10000
}
```

**Evaluation:**
```csharp
private bool EvaluateCondition(
    RequestApprovalStepTemplate stepTemplate,
    string formDataJson)
{
    if (string.IsNullOrEmpty(stepTemplate.ConditionExpression))
        return true;  // No condition = always include

    var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(formDataJson);
    var condition = JsonSerializer.Deserialize<ConditionExpression>(
        stepTemplate.ConditionExpression
    );

    // Evaluate condition logic
    return EvaluateExpression(condition, formData);
}
```

## Implementation Plan

### Sprint 1: Core Flexible Approver Assignment (Week 1-2)
- [ ] Create `ApproverType` enum
- [ ] Modify `RequestApprovalStepTemplate` entity
- [ ] Update database migration
- [ ] Modify `SubmitRequestCommandHandler` with new resolution logic
- [ ] Update `CreateRequestTemplateCommand` and handler
- [ ] Update frontend template creation form
- [ ] Add tests

### Sprint 2: Notification System (Week 3-4)
- [ ] Create `Notification` entity and `NotificationType` enum
- [ ] Implement `NotificationService`
- [ ] Create notification repository
- [ ] Add notification endpoints (GET, POST, PATCH)
- [ ] Integrate notifications into approval workflow
- [ ] Create `NotificationBell` component
- [ ] Create notifications page
- [ ] Add real-time updates (SignalR optional)

### Sprint 3: Pending Approvals View (Week 5)
- [ ] Create `GetPendingApprovalsQuery`
- [ ] Add controller endpoint
- [ ] Create frontend page `/dashboard/requests/pending-approvals`
- [ ] Add quick approve/reject actions
- [ ] Add badge to sidebar navigation

### Sprint 4: Advanced Features (Week 6+)
- [ ] Delegation system
- [ ] Escalation background service
- [ ] Parallel approval mode
- [ ] Conditional routing (optional)

## Database Migration

```sql
-- Add ApproverType to RequestApprovalStepTemplates
ALTER TABLE "RequestApprovalStepTemplates"
ADD COLUMN "ApproverType" VARCHAR(50) NOT NULL DEFAULT 'Role',
ADD COLUMN "SpecificUserId" UUID NULL,
ADD COLUMN "ApproverGroupId" UUID NULL,
ADD CONSTRAINT "FK_RequestApprovalStepTemplates_Users_SpecificUserId"
    FOREIGN KEY ("SpecificUserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
ADD CONSTRAINT "FK_RequestApprovalStepTemplates_RoleGroups_ApproverGroupId"
    FOREIGN KEY ("ApproverGroupId") REFERENCES "RoleGroups"("Id") ON DELETE SET NULL;

-- Make ApproverRole nullable (since it's only used for ApproverType.Role)
ALTER TABLE "RequestApprovalStepTemplates"
ALTER COLUMN "ApproverRole" DROP NOT NULL;

-- Create Notifications table
CREATE TABLE "Notifications" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Title" VARCHAR(500) NOT NULL,
    "Message" TEXT NOT NULL,
    "RelatedEntityType" VARCHAR(100) NULL,
    "RelatedEntityId" VARCHAR(100) NULL,
    "ActionUrl" VARCHAR(1000) NULL,
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "ReadAt" TIMESTAMP NULL,
    CONSTRAINT "FK_Notifications_Users_UserId"
        FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Notifications_UserId_IsRead" ON "Notifications"("UserId", "IsRead");
CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications"("CreatedAt" DESC);
```

## API Endpoints

### Notifications
- `GET /api/notifications` - Get user's notifications
- `GET /api/notifications/unread-count` - Get unread count
- `PATCH /api/notifications/{id}/mark-read` - Mark as read
- `PATCH /api/notifications/mark-all-read` - Mark all as read

### Pending Approvals
- `GET /api/requests/pending-approvals` - Get requests pending approval
- `POST /api/requests/{id}/approve` - Quick approve (existing)
- `POST /api/requests/{id}/reject` - Quick reject (existing)

## Success Metrics

1. **Flexibility**: Support 3 approver types (Role, User, Group)
2. **Visibility**: 100% of approvers receive notifications
3. **Efficiency**: Reduce average approval time by 30%
4. **User Satisfaction**: 90%+ approval from user testing

## References

- Jira Workflow: https://www.atlassian.com/software/jira/guides/workflows
- ServiceNow Approval Engine: https://docs.servicenow.com/
- Camunda BPMN: https://camunda.com/bpmn/

