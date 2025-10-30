# Requests System - Backend Architecture

## Struktura Katalogów

```
backend/
├── PortalForge.Domain/
│   ├── Entities/
│   │   ├── RequestTemplate.cs        # Szablon wniosku
│   │   ├── RequestTemplateField.cs   # Pole formularza
│   │   ├── RequestApprovalStepTemplate.cs  # Template etapu
│   │   ├── Request.cs                # Złożony wniosek
│   │   ├── RequestApprovalStep.cs    # Etap zatwierdzania
│   │   ├── QuizQuestion.cs           # Pytanie quizu
│   │   └── QuizAnswer.cs             # Odpowiedź na quiz
│   └── Enums/
│       ├── DepartmentRole.cs         # Employee, Manager, Director
│       ├── RequestStatus.cs          # Draft, InReview, Approved, etc.
│       ├── ApprovalStepStatus.cs     # Pending, InReview, Approved, etc.
│       ├── RequestPriority.cs        # Standard, Urgent
│       └── FieldType.cs              # Text, Textarea, Number, etc.
│
├── PortalForge.Application/
│   └── UseCases/
│       ├── RequestTemplates/
│       │   ├── Commands/
│       │   │   ├── CreateRequestTemplate/
│       │   │   │   ├── CreateRequestTemplateCommand.cs
│       │   │   │   ├── CreateRequestTemplateCommandHandler.cs
│       │   │   │   └── CreateRequestTemplateResult.cs
│       │   │   └── SeedRequestTemplates/
│       │   │       ├── SeedRequestTemplatesCommand.cs
│       │   │       ├── SeedRequestTemplatesCommandHandler.cs
│       │   │       └── SeedRequestTemplatesResult.cs
│       │   ├── Queries/
│       │   │   ├── GetRequestTemplates/
│       │   │   ├── GetAvailableRequestTemplates/
│       │   │   └── GetRequestTemplateById/
│       │   └── DTOs/
│       │       └── RequestTemplateDto.cs
│       └── Requests/
│           ├── Commands/
│           │   ├── SubmitRequest/
│           │   └── ApproveRequestStep/
│           ├── Queries/
│           │   ├── GetMyRequests/
│           │   └── GetRequestsToApprove/
│           └── DTOs/
│               └── RequestDto.cs
│
├── PortalForge.Infrastructure/
│   ├── Migrations/
│   │   └── 20251030200000_AddRequestsSystem.cs
│   ├── Persistence/Configurations/
│   │   ├── RequestTemplateConfiguration.cs
│   │   ├── RequestTemplateFieldConfiguration.cs
│   │   ├── RequestApprovalStepTemplateConfiguration.cs
│   │   ├── RequestConfiguration.cs
│   │   ├── RequestApprovalStepConfiguration.cs
│   │   ├── QuizQuestionConfiguration.cs
│   │   └── QuizAnswerConfiguration.cs
│   └── Repositories/
│       ├── RequestTemplateRepository.cs
│       └── RequestRepository.cs
│
├── PortalForge.Api/Controllers/
│   ├── RequestTemplatesController.cs
│   └── RequestsController.cs
│
└── PortalForge.Tests/Unit/
    ├── Requests/
    │   ├── SubmitRequestCommandHandlerTests.cs
    │   └── ApproveRequestStepCommandHandlerTests.cs
    └── RequestTemplates/
        └── GetAvailableRequestTemplatesQueryHandlerTests.cs
```

## Entities Relationships

```
RequestTemplate (1) ──< (N) RequestTemplateField
                   ──< (N) RequestApprovalStepTemplate
                   ──< (N) QuizQuestion
                   ──< (N) Request

Request (1) ──< (N) RequestApprovalStep
Request (N) ──> (1) RequestTemplate
Request (N) ──> (1) User (SubmittedBy)

RequestApprovalStep (1) ──< (N) QuizAnswer
RequestApprovalStep (N) ──> (1) User (Approver)

QuizAnswer (N) ──> (1) QuizQuestion
QuizAnswer (N) ──> (1) RequestApprovalStep
```

## Key Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)

**Commands (Write Operations):**
- `CreateRequestTemplate` - Tworzenie szablonu
- `SubmitRequest` - Złożenie wniosku
- `ApproveRequestStep` - Zatwierdzenie kroku
- `SeedRequestTemplates` - Seed danych

**Queries (Read Operations):**
- `GetRequestTemplates` - Lista wszystkich (admin)
- `GetAvailableRequestTemplates` - Dostępne dla użytkownika
- `GetRequestTemplateById` - Szczegóły szablonu
- `GetMyRequests` - Wnioski użytkownika
- `GetRequestsToApprove` - Wnioski do zatwierdzenia

### 2. Repository Pattern

```csharp
public interface IRequestTemplateRepository
{
    Task<RequestTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<RequestTemplate>> GetAllAsync();
    Task<IEnumerable<RequestTemplate>> GetActiveAsync();
    Task<IEnumerable<RequestTemplate>> GetAvailableForUserAsync(string? userDepartment);
    Task<Guid> CreateAsync(RequestTemplate template);
    Task UpdateAsync(RequestTemplate template);
    Task DeleteAsync(Guid id);
}
```

### 3. Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IRequestTemplateRepository RequestTemplateRepository { get; }
    IRequestRepository RequestRepository { get; }
    // ... other repositories
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Business Logic

### Automatic Approver Assignment

```csharp
// W SubmitRequestCommandHandler
foreach (var stepTemplate in orderedSteps)
{
    User? approver = null;
    
    if (stepTemplate.ApproverRole == DepartmentRole.Manager)
    {
        approver = submitter.Supervisor;  // Bezpośredni przełożony
    }
    else if (stepTemplate.ApproverRole == DepartmentRole.Director)
    {
        approver = submitter.Supervisor?.Supervisor;  // Przełożony przełożonego
    }

    if (approver != null)
    {
        var approvalStep = new RequestApprovalStep
        {
            ApproverId = approver.Id,
            Status = stepTemplate.StepOrder == 1 
                ? ApprovalStepStatus.InReview 
                : ApprovalStepStatus.Pending,
            RequiresQuiz = stepTemplate.RequiresQuiz
        };
        request.ApprovalSteps.Add(approvalStep);
    }
}
```

### Approval Progression Logic

```csharp
// W ApproveRequestStepCommandHandler
if (step.RequiresQuiz && step.QuizPassed != true)
{
    step.Status = ApprovalStepStatus.RequiresSurvey;
    request.Status = RequestStatus.AwaitingSurvey;
    return error;
}

step.Status = ApprovalStepStatus.Approved;
step.FinishedAt = DateTime.UtcNow;

var nextStep = request.ApprovalSteps
    .Where(s => s.Status == ApprovalStepStatus.Pending)
    .OrderBy(s => s.StepOrder)
    .FirstOrDefault();

if (nextStep != null)
{
    nextStep.Status = ApprovalStepStatus.InReview;
    nextStep.StartedAt = DateTime.UtcNow;
}
else
{
    request.Status = RequestStatus.Approved;
    request.CompletedAt = DateTime.UtcNow;
}
```

## Database Schema Highlights

### JSONB Usage

```csharp
// RequestTemplateField.Options
builder.Property(f => f.Options)
    .HasColumnType("jsonb");

// Request.FormData
builder.Property(r => r.FormData)
    .IsRequired()
    .HasColumnType("jsonb");

// QuizQuestion.Options
builder.Property(qq => qq.Options)
    .IsRequired()
    .HasColumnType("jsonb");
```

**Zalety:**
- Elastyczne przechowywanie zmiennych struktur
- Możliwość query'owania JSON w PostgreSQL
- Bez potrzeby dodatkowych tabel
- Wsparcie dla indeksów GIN

### Key Indexes

```csharp
// Performance optimization
builder.HasIndex(rt => rt.Category);
builder.HasIndex(rt => rt.DepartmentId);
builder.HasIndex(rt => rt.IsActive);
builder.HasIndex(r => r.Status);
builder.HasIndex(r => r.SubmittedById);
builder.HasIndex(ras => ras.ApproverId);
```

## Testing Strategy

### Unit Tests Structure

```csharp
[Fact]
public async Task Handle_ValidRequest_CreatesRequestSuccessfully()
{
    // Arrange - Setup mocks and test data
    var templateId = Guid.NewGuid();
    var template = new RequestTemplate { /* ... */ };
    _mockTemplateRepo.Setup(r => r.GetByIdAsync(templateId))
        .ReturnsAsync(template);
    
    // Act - Execute handler
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert - Verify results
    Assert.NotEqual(Guid.Empty, result.Id);
    _mockRepo.Verify(r => r.CreateAsync(It.IsAny<Request>()), Times.Once);
}
```

### Test Coverage Areas

- ✅ Valid inputs (happy path)
- ✅ Invalid inputs (null checks)
- ✅ Business rule validation
- ✅ Permission checks
- ✅ State transitions
- ✅ Quiz logic

## API Authorization

### Policy-Based Authorization

```csharp
[Authorize(Policy = "RequirePermission:requests.manage_templates")]
public async Task<ActionResult> Create([FromBody] CreateRequestTemplateCommand command)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    command.CreatedById = Guid.Parse(userId);
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

### Permission Checks

```csharp
// W startup configuration
services.AddAuthorization(options =>
{
    options.AddPolicy("RequirePermission:requests.view", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "requests.view")));
    // ... inne
});
```

## Performance Considerations

### Repository Optimization

```csharp
// Include related entities efficiently
public async Task<Request?> GetByIdAsync(Guid id)
{
    return await _context.Requests
        .Include(r => r.RequestTemplate)
        .Include(r => r.SubmittedBy)
        .Include(r => r.ApprovalSteps.OrderBy(aps => aps.StepOrder))
            .ThenInclude(aps => aps.Approver)
        .Include(r => r.ApprovalSteps)
            .ThenInclude(aps => aps.QuizAnswers)
        .FirstOrDefaultAsync(r => r.Id == id);
}
```

### AsNoTracking dla Read-Only

```csharp
// Queries don't need change tracking
public async Task<IEnumerable<RequestTemplate>> GetAllAsync()
{
    return await _context.RequestTemplates
        .Include(rt => rt.CreatedBy)
        .OrderByDescending(rt => rt.CreatedAt)
        .AsNoTracking()  // ← Performance boost
        .ToListAsync();
}
```

## Error Handling

```csharp
// Graceful error messages
if (template == null)
{
    throw new Exception("Request template not found");
}

if (step.ApproverId != command.ApproverId)
{
    return new ApproveRequestStepResult
    {
        Success = false,
        Message = "Unauthorized: You are not the approver for this step"
    };
}
```

## Validation

```csharp
// FluentValidation (if needed in future)
public class CreateRequestTemplateCommandValidator 
    : AbstractValidator<CreateRequestTemplateCommand>
{
    public CreateRequestTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);
        
        RuleFor(x => x.Fields)
            .NotEmpty().WithMessage("At least one field is required");
        
        RuleFor(x => x.PassingScore)
            .InclusiveBetween(0, 100)
            .When(x => x.PassingScore.HasValue);
    }
}
```

## Seeding Sample Data

5 przykładowych szablonów w `SeedRequestTemplatesCommandHandler`:

1. IT Equipment (IT dept, Manager → Director)
2. Training (All depts, Manager → Director + Quiz)
3. R&D Access (IT dept, Manager → Director + Quiz)
4. Vacation (All depts, Manager only)
5. Software License (All depts, Manager → Director)

## Extending the System

### Dodanie nowego typu pola

1. Dodaj do `FieldType` enum:
```csharp
public enum FieldType
{
    // ...existing
    File,  // Nowy typ
}
```

2. Update frontend field rendering
3. Update validation if needed

### Dodanie nowego etapu zatwierdzania

Możliwe poprzez UI - nie wymaga zmian w kodzie!

### Dodanie powiadomień email

```csharp
// W SubmitRequestCommandHandler po utworzeniu request
await _emailService.SendAsync(new Email
{
    To = firstApprover.Email,
    Subject = $"Nowy wniosek do zatwierdzenia: {request.RequestNumber}",
    Body = $"Użytkownik {submitter.FullName} złożył wniosek..."
});
```

## Monitoring & Debugging

### Logging Points

```csharp
_logger.LogInformation("Request {RequestNumber} submitted by {UserId}", 
    request.RequestNumber, submitter.Id);

_logger.LogInformation("Step {StepOrder} approved by {ApproverId} for request {RequestNumber}",
    step.StepOrder, approver.Id, request.RequestNumber);

_logger.LogWarning("Quiz failed for request {RequestNumber}, score: {Score}%",
    request.RequestNumber, step.QuizScore);
```

### Health Checks

```csharp
// Sprawdź czy są wnioski zawieszone
SELECT r."RequestNumber", ras."StepOrder" 
FROM "RequestApprovalSteps" ras
JOIN "Requests" r ON ras."RequestId" = r."Id"
WHERE ras."Status" = 'InReview' 
  AND ras."StartedAt" < NOW() - INTERVAL '7 days';
```

## Security Considerations

1. **Permission Checks**: Wszystkie endpointy wymagają odpowiednich permissions
2. **Approver Validation**: System weryfikuje czy użytkownik jest wyznaczonym zatwierdzającym
3. **Quiz Integrity**: Correct answers stored on backend, not exposed to frontend before submission
4. **Department Isolation**: Users see only templates for their departments
5. **Audit Trail**: All actions logged via existing AuditLog system

## Future Enhancements

1. **Versioning Templates**: Track template changes
2. **Request Delegation**: Approvers can delegate to others
3. **Bulk Operations**: Approve multiple requests at once
4. **Advanced Routing**: Custom approval logic (e.g., cost-based)
5. **File Attachments**: Support for uploading files
6. **Comments**: Discussion thread per request
7. **Notifications**: Email/push on status changes
8. **SLA Tracking**: Monitor vs estimated processing days
9. **Analytics**: Reports on request volume, approval times, etc.
10. **Request Templates Import/Export**: Share templates between environments

---

**Related Documentation:**
- [ADR 003](.ai/decisions/003-requests-system-architecture.md)
- [Progress Report](.ai/progress/2025-10-30-requests-system-implementation.md)
- [Quick Reference](.ai/requests-system-quick-reference.md)

