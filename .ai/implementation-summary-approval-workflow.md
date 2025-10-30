# Approval Workflow Enhancement - Implementation Summary

**Date:** 2025-10-30  
**Status:** ✅ Sprint 1 & 2 Complete  
**Backend:** ASP.NET Core 9.0 + PostgreSQL  
**Frontend:** Nuxt 3.17.7 + Vue 3.5.22

---

## 📋 Overview

Successfully implemented a comprehensive approval workflow enhancement system with:
1. **Flexible Approver Assignment** - Support for 4 types of approvers
2. **Real-time Notification System** - In-app notifications with polling
3. **Pending Approvals Dashboard** - Dedicated page for approvers
4. **Approve/Reject Actions** - Full workflow with comments and reasons

---

## 🎯 Implemented Features

### 1. Flexible Approver Assignment System

#### Backend
- **New Enum:** `ApproverType` with 4 values:
  - `Role` - Hierarchical role-based (Manager, Director)
  - `SpecificUser` - Specific user by ID
  - `UserGroup` - Any user from RoleGroup
  - `Submitter` - Self-approval/acknowledgment

- **Enhanced Entity:** `RequestApprovalStepTemplate`
  ```csharp
  public ApproverType ApproverType { get; set; } = ApproverType.Role;
  public DepartmentRole? ApproverRole { get; set; }  // Nullable
  public Guid? SpecificUserId { get; set; }
  public User? SpecificUser { get; set; }
  public Guid? ApproverGroupId { get; set; }
  public RoleGroup? ApproverGroup { get; set; }
  ```

- **Smart Approver Resolution:** `SubmitRequestCommandHandler.ResolveApproversAsync()`
  - Resolves approvers based on `ApproverType`
  - Supports multiple approvers per step (for UserGroup)
  - Hierarchical resolution for Role type

#### Frontend
- **Updated Types:** `ApproverType` enum and `RequestApprovalStepTemplate` interface
- **Ready for UI:** Template creation form can be extended to support approver type selection

---

### 2. Notification System

#### Backend Components

**New Entity:** `Notification`
```csharp
public Guid Id { get; set; }
public Guid UserId { get; set; }
public NotificationType Type { get; set; }
public string Title { get; set; }
public string Message { get; set; }
public string? RelatedEntityType { get; set; }
public string? RelatedEntityId { get; set; }
public string? ActionUrl { get; set; }
public bool IsRead { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? ReadAt { get; set; }
```

**Notification Types:**
- `RequestPendingApproval` - New request awaiting approval
- `RequestApproved` - Request step approved
- `RequestRejected` - Request rejected
- `RequestCompleted` - All steps approved
- `RequestCommented` - Comment added
- `System` - System notifications
- `Announcement` - Announcements

**Service Layer:**
- `INotificationService` with methods:
  - `CreateNotificationAsync()` - Create custom notification
  - `NotifyApproverAsync()` - Notify approver of pending request
  - `NotifySubmitterAsync()` - Notify submitter of status change
  - `GetUserNotificationsAsync()` - Get user's notifications (paginated)
  - `GetUnreadCountAsync()` - Get unread count
  - `MarkAsReadAsync()` - Mark single as read
  - `MarkAllAsReadAsync()` - Mark all as read

**Repository:**
- `INotificationRepository` + `NotificationRepository`
- Efficient queries with indexes on `UserId + IsRead` and `CreatedAt`

**Integration Points:**
- `SubmitRequestCommandHandler` - Notifies first step approvers
- `ApproveRequestStepCommandHandler` - Notifies next approver or submitter
- `RejectRequestStepCommandHandler` - Notifies submitter of rejection

#### API Endpoints

**NotificationsController:**
- `GET /api/notifications` - Get user's notifications (with pagination)
- `GET /api/notifications/unread-count` - Get unread count
- `PATCH /api/notifications/{id}/mark-read` - Mark as read
- `PATCH /api/notifications/mark-all-read` - Mark all as read

**RequestsController (Extended):**
- `GET /api/requests/pending-approvals` - Get requests awaiting user's approval
- `POST /api/requests/{id}/steps/{stepId}/reject` - Reject request step

#### Frontend Components

**Pinia Store:** `useNotificationsStore`
```typescript
state: {
  notifications: Notification[]
  unreadCount: number
  loading: boolean
  error: string | null
}

actions: {
  fetchNotifications(unreadOnly)
  fetchUnreadCount()
  markAsRead(notificationId)
  markAllAsRead()
  startPolling(intervalMs) // Auto-refresh every 30s
}
```

**NotificationBell Component:**
- Bell icon with unread count badge
- Dropdown with notification list
- Click notification to navigate to related entity
- Mark as read on click
- "Mark all as read" button
- Auto-polling every 30 seconds
- Integrated into `layouts/default.vue` header

**Features:**
- Real-time unread count
- Icon and color based on notification type
- Relative time display ("2 min temu", "3 godz. temu")
- Click-outside to close
- Smooth transitions

---

### 3. Pending Approvals Dashboard

**Page:** `/dashboard/requests/pending-approvals`

**Features:**
- List of requests awaiting user's approval
- Request info: number, template, submitter, date, priority
- Current step indicator
- Quick actions:
  - ✅ **Approve** - With optional comment
  - ❌ **Reject** - With required reason
  - 📄 **View Details** - Navigate to full request view
- Modal dialogs for approve/reject
- Real-time updates after action
- Empty state when no pending approvals

**UI/UX:**
- Priority badges (Urgent = red, Standard = blue)
- Yellow highlight for pending items
- Loading states
- Error handling
- Responsive design

---

## 📁 Files Created/Modified

### Backend

**New Files (Domain):**
- `backend/PortalForge.Domain/Enums/ApproverType.cs`
- `backend/PortalForge.Domain/Enums/NotificationType.cs`
- `backend/PortalForge.Domain/Entities/Notification.cs`

**New Files (Application):**
- `backend/PortalForge.Application/Services/INotificationService.cs`
- `backend/PortalForge.Application/Interfaces/INotificationRepository.cs`
- `backend/PortalForge.Application/UseCases/Requests/Commands/RejectRequestStep/*` (3 files)
- `backend/PortalForge.Application/UseCases/Requests/Queries/GetPendingApprovals/*` (3 files)
- `backend/PortalForge.Application/UseCases/Notifications/DTOs/NotificationDto.cs`
- `backend/PortalForge.Application/UseCases/Notifications/Queries/GetUserNotifications/*` (3 files)
- `backend/PortalForge.Application/UseCases/Notifications/Queries/GetUnreadCount/*` (3 files)
- `backend/PortalForge.Application/UseCases/Notifications/Commands/MarkAsRead/*` (2 files)
- `backend/PortalForge.Application/UseCases/Notifications/Commands/MarkAllAsRead/*` (2 files)

**New Files (Infrastructure):**
- `backend/PortalForge.Infrastructure/Services/NotificationService.cs`
- `backend/PortalForge.Infrastructure/Persistence/Repositories/NotificationRepository.cs`
- `backend/PortalForge.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs`
- `backend/PortalForge.Infrastructure/Migrations/20251030154207_AddApprovalWorkflowEnhancements.cs`

**New Files (API):**
- `backend/PortalForge.Api/Controllers/NotificationsController.cs`

**Modified Files:**
- `backend/PortalForge.Domain/Entities/RequestApprovalStepTemplate.cs` - Added flexible approver fields
- `backend/PortalForge.Infrastructure/Persistence/ApplicationDbContext.cs` - Added Notifications DbSet
- `backend/PortalForge.Infrastructure/Persistence/Configurations/RequestApprovalStepTemplateConfiguration.cs` - New relationships
- `backend/PortalForge.Application/Common/Interfaces/IUnitOfWork.cs` - Added NotificationRepository
- `backend/PortalForge.Infrastructure/Repositories/UnitOfWork.cs` - Implemented NotificationRepository
- `backend/PortalForge.Infrastructure/DependencyInjection.cs` - Registered NotificationService
- `backend/PortalForge.Application/UseCases/Requests/Commands/SubmitRequest/SubmitRequestCommandHandler.cs` - Approver resolution + notifications
- `backend/PortalForge.Application/UseCases/Requests/Commands/ApproveRequestStep/ApproveRequestStepCommandHandler.cs` - Added notifications
- `backend/PortalForge.Api/Controllers/RequestsController.cs` - Added pending-approvals and reject endpoints

### Frontend

**New Files:**
- `frontend/stores/notifications.ts` - Pinia store
- `frontend/components/NotificationBell.vue` - Bell component
- `frontend/pages/dashboard/requests/pending-approvals.vue` - Approvals page

**Modified Files:**
- `frontend/types/requests.ts` - Added ApproverType, NotificationType, Notification, RejectStepDto, updated interfaces
- `frontend/composables/useRequestsApi.ts` - Added notification and pending approval methods
- `frontend/layouts/default.vue` - Integrated NotificationBell component

---

## 🗄️ Database Changes

**Migration:** `20251030154207_AddApprovalWorkflowEnhancements`

**Changes to `RequestApprovalStepTemplates`:**
- Made `ApproverRole` nullable
- Added `ApproverType` (int, default 0 = Role)
- Added `SpecificUserId` (uuid, nullable, FK to Users)
- Added `ApproverGroupId` (uuid, nullable, FK to RoleGroups)

**New Table: `Notifications`**
- `Id` (uuid, PK)
- `UserId` (uuid, FK to Users, indexed)
- `Type` (int)
- `Title` (varchar 200)
- `Message` (text)
- `RelatedEntityType` (varchar 100, nullable)
- `RelatedEntityId` (varchar 100, nullable)
- `ActionUrl` (varchar 500, nullable)
- `IsRead` (boolean, default false)
- `CreatedAt` (timestamp, indexed descending)
- `ReadAt` (timestamp, nullable)

**Indexes:**
- `IX_Notifications_UserId_IsRead` - Composite for efficient unread queries
- `IX_Notifications_CreatedAt` - Descending for recent-first ordering

---

## 🚀 How to Use

### For Developers

1. **Backend is running** on `http://localhost:5155`
2. **Frontend is running** on `http://localhost:3000/portalforge/fe/`
3. **Database migration applied** - All tables and columns created

### For Users

1. **Submit a Request:**
   - Go to `/dashboard/requests`
   - Select template and fill form
   - On submit, first approver receives notification

2. **Receive Notification:**
   - Bell icon in header shows unread count
   - Click bell to see notifications
   - Click notification to go to request

3. **Approve/Reject:**
   - Go to `/dashboard/requests/pending-approvals`
   - See all requests awaiting your approval
   - Click "Zatwierdź" or "Odrzuć"
   - Add comment/reason
   - Next approver or submitter gets notified

---

## 🎨 UI/UX Highlights

- **Real-time Updates:** Polling every 30 seconds
- **Visual Feedback:** Unread badge, colored icons, priority badges
- **Smooth Animations:** Dropdown transitions, loading states
- **Dark Mode Support:** All components support dark mode
- **Responsive:** Works on mobile and desktop
- **Accessibility:** ARIA labels, keyboard navigation

---

---

## 🎨 Sprint 3: UI for Approver Type Selection - ✅ COMPLETE

### Implemented Features:

1. **Created `useUsersApi` composable:**
   - `getUsers()` method with filtering and pagination
   - `getUserById()` method
   - Uses types from `stores/admin.ts`

2. **Extended `useRoleGroupApi` composable:**
   - Added `getAllRoleGroups()` method
   - Returns list of all role groups

3. **Created `ApprovalStepEditor.vue` component:**
   - Approver type selection dropdown (Role, SpecificUser, UserGroup, Submitter)
   - Conditional field display based on type:
     - **Role**: Dropdown with roles (Manager, Director)
     - **SpecificUser**: User search with autocomplete
     - **UserGroup**: Dropdown with user groups
     - **Submitter**: Info about self-approval
   - "Requires quiz" checkbox
   - Remove step button

4. **Updated template creation page:**
   - Loads users and groups on mount
   - Uses `ApprovalStepEditor` component instead of simple form
   - Loading state while fetching data

### Files Created/Modified:

**Created:**
- `frontend/composables/useUsersApi.ts`
- `frontend/components/ApprovalStepEditor.vue`

**Modified:**
- `frontend/composables/useRoleGroupApi.ts` - added `getAllRoleGroups()`
- `frontend/pages/admin/request-templates/create.vue` - integrated ApprovalStepEditor
- `frontend/components/NotificationBell.vue` - fixed TypeScript error
- `frontend/types/requests.ts` - added `isActive` to `CreateRequestTemplateDto`

---

## 🔜 Next Steps (Sprint 4 & Beyond)

1. **Update Edit Template Form:** Apply same changes to `edit/[id].vue`
2. **Delegation:** Allow approvers to delegate to another user
3. **Escalation:** Auto-escalate if not approved within SLA
4. **Parallel Approval:** Require all/any from group
5. **Conditional Routing:** Different paths based on form data
6. **Email Notifications:** Send emails in addition to in-app
7. **Audit Trail:** Log all approval actions
8. **Analytics:** Dashboard with approval metrics

---

## ✅ Testing Checklist

- [x] Backend compiles without errors
- [x] Database migration applied successfully
- [x] Frontend compiles without TypeScript errors
- [x] Backend running on port 5155
- [x] Frontend running on port 3000
- [ ] End-to-end test: Submit request → Receive notification → Approve → Next approver notified
- [ ] End-to-end test: Submit request → Reject → Submitter notified
- [ ] Test all 4 approver types (Role, SpecificUser, UserGroup, Submitter)
- [ ] Test notification polling
- [ ] Test mark as read functionality
- [ ] Test pending approvals page

---

## 📚 Documentation

- Design document: `.ai/decisions/004-approval-workflow-enhancement.md`
- This summary: `.ai/implementation-summary-approval-workflow.md`
- API documentation: Swagger UI at `http://localhost:5155/swagger`

---

**Implementation completed successfully! 🎉**

