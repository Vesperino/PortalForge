# Plan Implementacji: Pełny System Wnioskowy z Zarządzaniem Urlopami i L4

**Data utworzenia:** 2025-11-03
**Wersja:** 1.0.0
**Szacowany czas realizacji:** 160-180h (4-5 tygodni dla 1 developera)

---

## 🎯 Zakres projektu

Rozbudowa systemu wnioskowego o:

- Pełne zarządzanie urlopami (26 dni + urlop zaległy + na żądanie + okolicznościowy)
- System L4 (zgłoszenia chorobowe, ZUS)
- Okres próbny i proporcjonalne naliczanie urlopów
- Kalendarz urlopów zespołu
- System komentarzy i edycji wniosków
- SLA i przypomnienia dla przełożonych
- Zastępcy kierowników/dyrektorów
- Audit log wszystkich ważnych akcji
- Eksport raportów
- **Pełna zgodność z polskim prawem pracy 2025**

---

## 📚 Wymagania biznesowe

### Urlopy - zgodność z prawem polskim

1. **Podstawowa pula:** 26 dni urlopu rocznie (dla stażu > 10 lat może być więcej)
2. **Proporcjonalne naliczanie:**
   - Pracownik zatrudniony w trakcie roku: `(26 / 12) * pozostałe miesiące`
   - Zaokrąglanie: **w górę do pełnych dni** (Art. 155¹ KP)
   - Okres próbny: **2.17 dnia/miesiąc** (26/12)
3. **Urlop zaległy:**
   - Niewykorzystany urlop można wykorzystać **do 30 września** następnego roku
   - System musi trackować osobno urlop z bieżącego i poprzedniego roku
   - Auto-przypomnienia przed wygaśnięciem (1 września)
4. **Urlop na żądanie:**
   - 4 dni rocznie
   - Bez wcześniejszego wniosku (składany w dniu)
   - Auto-akceptacja jeśli user ma wystarczająco dni w puli
5. **Urlop okolicznościowy:**
   - 2 dni - ślub własny/dziecka, pogrzeb bliskiego, narodziny dziecka
   - Osobna pula (nie z 26 dni)
   - Wymaga akceptacji przełożonego

### L4 (Zwolnienie lekarskie)

1. **Auto-akceptacja:** Prawo polskie - pracodawca nie może odmówić L4
2. **Do wiadomości przełożonego:** Przełożony dostaje powiadomienie
3. **Zgłoszenie retrospektywne:** Do 14 dni wstecz
4. **ZUS:** Po 33 dniach L4 wymagane zaświadczenie ZUS (system przypomina)
5. **Osobna tabela:** `SickLeave` (analogicznie do `VacationSchedule`)

### System wnioskowy

1. **Walidacja struktury:** Przed wysłaniem sprawdzamy czy user ma wymaganych przełożonych
2. **Zastępcy:** Jeśli kierownik/dyrektor na urlopie → wniosek trafia do zastępcy
3. **Komentarze:** Przełożony i składający mogą dodawać komentarze, załączniki
4. **Edycja wniosku:** Składający może edytować (historia zmian zachowana)
5. **Anulowanie urlopu:**
   - Przełożony: do 1 dnia po rozpoczęciu
   - Admin: zawsze
6. **SLA:** Przypomnienia dla przełożonych (po 3 dniach), wizualne oznaczenie "overdue"

### Kalendarz urlopów

1. **Widoczność:** Każdy pracownik widzi kalendarz swojego działu
2. **Dla kierowników:** Widok przed zatwierdzeniem wniosku (walidacja obsady)
3. **Eksport:** PDF/Excel raportów urlopów

### Przejście między działami

1. **Przepięcie wniosków:** Wnioski w trakcie rozpatrywania → nowy przełożony
2. **Zachowanie puli urlopów:** Limit urlopów pozostaje bez zmian

---

## 📋 FAZA 1: Fundament systemu urlopowego (Backend + Database)

### 1.1 Rozszerzenie modelu `User.cs`

**Plik:** `backend/PortalForge.Domain/Entities/User.cs`

```csharp
// Urlopy - pula bieżąca
public int AnnualVacationDays { get; set; } = 26;
public int VacationDaysUsed { get; set; } = 0;
public int OnDemandVacationDaysUsed { get; set; } = 0; // max 4
public int CircumstantialLeaveDaysUsed { get; set; } = 0;

// Urlopy - z poprzedniego roku
public int CarriedOverVacationDays { get; set; } = 0;
public DateTime? CarriedOverExpiryDate { get; set; } // 30 września

// Computed properties
public int TotalAvailableVacationDays => AnnualVacationDays + CarriedOverVacationDays - VacationDaysUsed;

// Zatrudnienie
public DateTime? EmploymentStartDate { get; set; }
public bool IsOnProbation { get; set; } = false;
public DateTime? ProbationEndDate { get; set; }
public int YearsOfService => EmploymentStartDate.HasValue
    ? (DateTime.UtcNow.Year - EmploymentStartDate.Value.Year) : 0;

// Ustawienia powiadomień
public bool EmailNotificationsEnabled { get; set; } = true;
```

**Zgodność z CLAUDE.md:**

- ✅ Clean Architecture - Domain layer
- ✅ Computed properties dla czytelności
- ✅ Domyślne wartości zgodne z prawem polskim

---

### 1.2 Rozszerzenie modelu `Department.cs`

**Plik:** `backend/PortalForge.Domain/Entities/Department.cs`

```csharp
public Guid? HeadOfDepartmentSubstituteId { get; set; }
public User? HeadOfDepartmentSubstitute { get; set; }

public Guid? DirectorSubstituteId { get; set; }
public User? DirectorSubstitute { get; set; }
```

**Zgodność z CLAUDE.md:**

- ✅ Navigation properties dla EF Core
- ✅ Nullable - zastępca opcjonalny

---

### 1.3 Nowa tabela `SickLeave.cs`

**Plik:** `backend/PortalForge.Domain/Entities/SickLeave.cs`

```csharp
public class SickLeave
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysCount { get; set; }

    public bool RequiresZusDocument { get; set; } // po 33 dniach
    public string? ZusDocumentUrl { get; set; }

    public Guid SourceRequestId { get; set; }
    public Request SourceRequest { get; set; }

    public SickLeaveStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}

public enum SickLeaveStatus
{
    Active,     // L4 w trakcie
    Completed,  // L4 zakończone
    Cancelled   // L4 anulowane
}
```

**Zgodność z CLAUDE.md:**

- ✅ Domain entity - czysta logika biznesowa
- ✅ Enum dla statusów (type-safe)
- ✅ Navigation properties do User i Request

---

### 1.4 Rozszerzenie `Request.cs` i `RequestTemplate.cs`

**Pliki:**

- `backend/PortalForge.Domain/Entities/Request.cs`
- `backend/PortalForge.Domain/Entities/RequestTemplate.cs`

```csharp
// Do Request.cs
public LeaveType? LeaveType { get; set; }
public string? Attachments { get; set; } // JSON array URLs

// Do RequestTemplate.cs
public bool AllowsAttachments { get; set; }
public int? MaxRetrospectiveDays { get; set; } // ile dni wstecz można składać
public bool IsVacationRequest { get; set; }
public bool IsSickLeaveRequest { get; set; }

// Nowy enum w Domain/Enums/LeaveType.cs
public enum LeaveType
{
    Annual,          // Urlop wypoczynkowy
    OnDemand,        // Urlop na żądanie
    Circumstantial,  // Urlop okolicznościowy
    Sick,            // L4
    Other            // Inne
}
```

**Zgodność z CLAUDE.md:**

- ✅ JSON dla dynamicznych danych (Attachments)
- ✅ Flags dla typów wniosków (boolean)
- ✅ Enum w osobnym pliku Domain/Enums

---

### 1.5 Nowa tabela `RequestComment.cs`

**Plik:** `backend/PortalForge.Domain/Entities/RequestComment.cs`

```csharp
public class RequestComment
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Request Request { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public string Comment { get; set; }
    public string? Attachments { get; set; } // JSON array
    public DateTime CreatedAt { get; set; }
}
```

**Zgodność z CLAUDE.md:**

- ✅ Relacja many-to-one z Request
- ✅ CreatedAt dla auditu

---

### 1.6 Nowa tabela `RequestEditHistory.cs`

**Plik:** `backend/PortalForge.Domain/Entities/RequestEditHistory.cs`

```csharp
public class RequestEditHistory
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Request Request { get; set; }

    public Guid EditedByUserId { get; set; }
    public User EditedBy { get; set; }

    public DateTime EditedAt { get; set; }
    public string OldFormData { get; set; } // JSON
    public string NewFormData { get; set; } // JSON
    public string? ChangeReason { get; set; }
}
```

**Zgodność z CLAUDE.md:**

- ✅ Full audit trail
- ✅ JSON dla FormData (zachowanie struktury)

---

### 1.7 Nowa tabela `AuditLog.cs`

**Plik:** `backend/PortalForge.Domain/Entities/AuditLog.cs`

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } // "User", "Request", "VacationSchedule"
    public string EntityId { get; set; }
    public string Action { get; set; } // "VacationAllowanceUpdated", "RequestCancelled"

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
```

**Zgodność z CLAUDE.md:**

- ✅ Generyczny audit log (nie tylko dla User)
- ✅ Nullable UserId (dla system actions)
- ✅ IP address dla security audit

---

### 1.8 Migracje EF Core (9 migracji)

**Pliki:** `backend/PortalForge.Infrastructure/Migrations/`

1. `AddVacationFieldsToUser` - Pola urlopowe w User
2. `AddEmploymentFieldsToUser` - EmploymentStartDate, IsOnProbation, etc.
3. `AddSubstituteFieldsToDepartment` - Zastępcy w Department
4. `CreateSickLeaveTable` - Tabela SickLeave
5. `AddLeaveTypeAndAttachmentsToRequest` - LeaveType, Attachments w Request
6. `AddTemplateVacationFlags` - IsVacationRequest, IsSickLeaveRequest w RequestTemplate
7. `CreateRequestCommentTable` - Tabela RequestComment
8. `CreateRequestEditHistoryTable` - Tabela RequestEditHistory
9. `CreateAuditLogTable` - Tabela AuditLog

**Komenda:**

```bash
cd backend/PortalForge.Api
dotnet ef migrations add AddVacationFieldsToUser --project ../PortalForge.Infrastructure
# ... powtórz dla pozostałych 8 migracji
dotnet ef database update --project ../PortalForge.Infrastructure
```

**Zgodność z CLAUDE.md:**

- ✅ Osobne migracje dla każdej logicznej zmiany
- ✅ Opisowe nazwy migracji

---

## 📋 FAZA 2: Serwisy biznesowe (Backend Logic)

### 2.1 `VacationCalculationService.cs`

**Plik:** `backend/PortalForge.Application/Services/VacationCalculationService.cs`

**Interface:** `backend/PortalForge.Application/Interfaces/IVacationCalculationService.cs`

```csharp
public interface IVacationCalculationService
{
    /// <summary>
    /// Oblicza proporcjonalną liczbę dni urlopu zgodnie z polskim Kodeksem Pracy
    /// </summary>
    int CalculateProportionalVacationDays(DateTime employmentStartDate, int annualDays = 26);

    /// <summary>
    /// Sprawdza czy użytkownik może wziąć urlop w podanych datach
    /// </summary>
    Task<(bool CanTake, string? ErrorMessage)> CanTakeVacationAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        LeaveType leaveType);

    /// <summary>
    /// Oblicza liczbę wykorzystanych dni urlopu w danym roku
    /// </summary>
    Task<int> CalculateVacationDaysUsedAsync(Guid userId, int year);

    /// <summary>
    /// Pobiera dostępne dni urlopu (bieżący rok + zaległy)
    /// </summary>
    Task<int> GetAvailableVacationDaysAsync(Guid userId);
}
```

**Implementacja kluczowych metod:**

```csharp
public int CalculateProportionalVacationDays(DateTime employmentStartDate, int annualDays = 26)
{
    var currentYear = DateTime.UtcNow.Year;
    var startYear = employmentStartDate.Year;

    // Jeśli zatrudniony w poprzednich latach - pełna pula
    if (startYear < currentYear)
        return annualDays;

    // Proporcjonalnie za pozostałe miesiące
    var monthsRemaining = 12 - employmentStartDate.Month + 1;
    var proportionalDays = (annualDays / 12.0) * monthsRemaining;

    // Zaokrąglanie w górę zgodnie z Art. 155¹ KP
    return (int)Math.Ceiling(proportionalDays);
}

public async Task<(bool CanTake, string? ErrorMessage)> CanTakeVacationAsync(
    Guid userId,
    DateTime startDate,
    DateTime endDate,
    LeaveType leaveType)
{
    var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
    if (user == null)
        return (false, "Użytkownik nie istnieje");

    var requestedDays = (endDate - startDate).Days + 1;

    switch (leaveType)
    {
        case LeaveType.OnDemand:
            if (user.OnDemandVacationDaysUsed >= 4)
                return (false, "Wykorzystano już 4 dni urlopu na żądanie w tym roku");
            if (user.OnDemandVacationDaysUsed + requestedDays > 4)
                return (false, $"Możesz wziąć jeszcze {4 - user.OnDemandVacationDaysUsed} dni urlopu na żądanie");
            break;

        case LeaveType.Circumstantial:
            // Limit 2 dni na wydarzenie (można rozszerzyć)
            if (requestedDays > 2)
                return (false, "Urlop okolicznościowy to maksymalnie 2 dni");
            break;

        case LeaveType.Annual:
            var availableDays = user.TotalAvailableVacationDays;
            if (requestedDays > availableDays)
                return (false, $"Brak wystarczającej liczby dni urlopu. Dostępne: {availableDays} dni");
            break;
    }

    return (true, null);
}
```

**Zgodność z CLAUDE.md:**

- ✅ Interface + Implementation (Dependency Inversion)
- ✅ XML documentation
- ✅ Async/await dla operacji I/O
- ✅ Tuple return dla walidacji (bool + error message)

---

### 2.2 `AuditLogService.cs`

**Plik:** `backend/PortalForge.Infrastructure/Services/AuditLogService.cs`

**Interface:** `backend/PortalForge.Application/Interfaces/IAuditLogService.cs`

```csharp
public interface IAuditLogService
{
    Task LogActionAsync(
        string entityType,
        string entityId,
        string action,
        Guid? userId = null,
        string? oldValue = null,
        string? newValue = null,
        string? reason = null,
        string? ipAddress = null);

    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        string? entityType = null,
        string? action = null,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<byte[]> ExportAuditLogAsync(/* filters */);
}
```

**Implementacja:**

```csharp
public async Task LogActionAsync(
    string entityType,
    string entityId,
    string action,
    Guid? userId = null,
    string? oldValue = null,
    string? newValue = null,
    string? reason = null,
    string? ipAddress = null)
{
    var auditLog = new AuditLog
    {
        Id = Guid.NewGuid(),
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        UserId = userId,
        OldValue = oldValue,
        NewValue = newValue,
        Reason = reason,
        IpAddress = ipAddress,
        Timestamp = DateTime.UtcNow
    };

    await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
    await _unitOfWork.SaveChangesAsync();

    _logger.LogInformation(
        "Audit log created: {EntityType} {EntityId} - {Action} by User {UserId}",
        entityType, entityId, action, userId);
}
```

**Zgodność z CLAUDE.md:**

- ✅ Structured logging (Serilog)
- ✅ Repository pattern przez UnitOfWork
- ✅ Async all the way

---

### 2.3 Rozszerzenie `RequestRoutingService.cs`

**Plik:** `backend/PortalForge.Application/Services/RequestRoutingService.cs`

**Nowe metody:**

```csharp
/// <summary>
/// Waliduje czy użytkownik ma wymaganą strukturę przełożonych dla danego szablonu
/// </summary>
public async Task<(bool IsValid, List<string> Errors)> ValidateApprovalStructureAsync(
    Guid userId,
    IEnumerable<RequestApprovalStepTemplate> stepTemplates)
{
    var errors = new List<string>();
    var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

    foreach (var stepTemplate in stepTemplates)
    {
        switch (stepTemplate.ApproverType)
        {
            case ApproverType.DirectSupervisor:
                if (user.SupervisorId == null)
                    errors.Add("Nie masz przypisanego bezpośredniego przełożonego w strukturze organizacyjnej");
                break;

            case ApproverType.Role:
                var requiredRole = stepTemplate.RequiredRole;
                var approver = await FindApproverByRoleAsync(userId, requiredRole);
                if (approver == null)
                    errors.Add($"Nie znaleziono przełożonego z rolą: {requiredRole}");
                break;

            case ApproverType.SpecificDepartment:
                var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(stepTemplate.DepartmentId.Value);
                if (department?.HeadOfDepartmentId == null && department?.DirectorId == null)
                    errors.Add($"Dział '{department?.Name}' nie ma przypisanego kierownika ani dyrektora");
                break;
        }
    }

    return (errors.Count == 0, errors);
}

/// <summary>
/// Pobiera approver'a dla kroku, uwzględniając zastępców jeśli przełożony jest niedostępny
/// </summary>
public async Task<Guid?> GetApproverForStepWithSubstituteAsync(
    Guid userId,
    RequestApprovalStepTemplate stepTemplate)
{
    var primaryApproverId = await GetApproverIdAsync(userId, stepTemplate);
    if (primaryApproverId == null)
        return null;

    // Sprawdź czy przełożony jest na urlopie
    var isOnVacation = await _unitOfWork.VacationScheduleRepository
        .IsUserOnVacationAsync(primaryApproverId.Value, DateTime.UtcNow);

    if (!isOnVacation)
        return primaryApproverId;

    // Szukaj zastępcy
    switch (stepTemplate.ApproverType)
    {
        case ApproverType.DirectSupervisor:
            var supervisor = await _unitOfWork.UserRepository.GetByIdAsync(primaryApproverId.Value);
            return supervisor?.SupervisorId; // Przełożony przełożonego

        case ApproverType.SpecificDepartment:
            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(stepTemplate.DepartmentId.Value);
            if (primaryApproverId == department?.HeadOfDepartmentId)
                return department?.HeadOfDepartmentSubstituteId;
            if (primaryApproverId == department?.DirectorId)
                return department?.DirectorSubstituteId;
            break;
    }

    return null; // Brak zastępcy - zwracamy błąd
}
```

**Zgodność z CLAUDE.md:**

- ✅ Guard clauses (early returns)
- ✅ Tuple return dla walidacji
- ✅ XML documentation
- ✅ Separation of concerns

---

### 2.4 Rozszerzenie `NotificationService.cs`

**Plik:** `backend/PortalForge.Infrastructure/Services/NotificationService.cs`

**Nowe typy powiadomień w enum:**

```csharp
// Domain/Enums/NotificationType.cs
public enum NotificationType
{
    // Istniejące...
    RequestPendingApproval,
    RequestApproved,
    RequestRejected,

    // Nowe
    RequestCommentAdded,
    RequestEdited,
    ApprovalDeadlineApproaching,
    VacationAllowanceUpdated,
    CarriedOverVacationExpiring,
    VacationCancelled,
    SickLeaveSubmitted
}
```

**Nowe metody:**

```csharp
public async Task NotifyRequestCommentAsync(Guid requestId, Guid commentAuthorId)
{
    var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
    var author = await _unitOfWork.UserRepository.GetByIdAsync(commentAuthorId);

    // Powiadom składającego
    if (request.SubmittedById != commentAuthorId)
    {
        await CreateNotificationAsync(
            request.SubmittedById,
            NotificationType.RequestCommentAdded,
            "Nowy komentarz do wniosku",
            $"{author.FirstName} {author.LastName} dodał(a) komentarz do wniosku {request.RequestNumber}",
            "Request",
            requestId.ToString(),
            $"/dashboard/requests/{requestId}");
    }

    // Powiadom wszystkich approverów
    var approvers = request.ApprovalSteps
        .Where(s => s.ApproverId != commentAuthorId)
        .Select(s => s.ApproverId)
        .Distinct();

    foreach (var approverId in approvers)
    {
        await CreateNotificationAsync(approverId, /* ... */);
    }
}

public async Task NotifyApprovalDeadlineAsync(Guid requestId, Guid approverId)
{
    var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);

    await CreateNotificationAsync(
        approverId,
        NotificationType.ApprovalDeadlineApproaching,
        "Przypomnienie o wniosku",
        $"Wniosek {request.RequestNumber} oczekuje na rozpatrzenie już 3 dni",
        "Request",
        requestId.ToString(),
        $"/dashboard/requests/{requestId}");
}
```

**Rozszerzenie CreateNotificationAsync o EmailNotificationsEnabled:**

```csharp
private async Task CreateNotificationAsync(/* ... */)
{
    // ... create notification in DB ...

    // Sprawdź ustawienia użytkownika przed wysyłką email
    var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

    if (user.EmailNotificationsEnabled && ShouldSendEmail(notificationType))
    {
        await _emailService.SendNotificationEmailAsync(user.Email, title, message, actionUrl);
    }
}

private bool ShouldSendEmail(NotificationType type)
{
    // Tylko ważne powiadomienia mailem
    return type switch
    {
        NotificationType.RequestPendingApproval => true,
        NotificationType.RequestApproved => true,
        NotificationType.RequestRejected => true,
        NotificationType.VacationCancelled => true,
        NotificationType.ApprovalDeadlineApproaching => true,
        _ => false // Reszta tylko in-app
    };
}
```

**Zgodność z CLAUDE.md:**

- ✅ Switch expression (C# 8+)
- ✅ Sprawdzanie user preferences
- ✅ Separation of concerns (email logic w EmailService)

---

### 2.5 Nowy `FileStorageService.cs`

**Plik:** `backend/PortalForge.Infrastructure/Services/FileStorageService.cs`

**Interface:** `backend/PortalForge.Application/Interfaces/IFileStorageService.cs`

```csharp
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task<string> GetFileUrlAsync(string filePath);
    Task DeleteFileAsync(string filePath);
}
```

**Implementacja:**

```csharp
public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        // WAŻNE: NIE hardcodować ścieżek! Użyć konfiguracji
        var basePath = _configuration["Storage:BasePath"]
            ?? throw new InvalidOperationException("Storage:BasePath not configured");

        var folderPath = Path.Combine(basePath, folder);
        Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStreamOutput = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        _logger.LogInformation("File saved: {FileName} to {Path}", fileName, fullPath);

        return Path.Combine(folder, uniqueFileName); // Relative path
    }

    public Task<string> GetFileUrlAsync(string filePath)
    {
        var baseUrl = _configuration["Storage:BaseUrl"];
        return Task.FromResult($"{baseUrl}/{filePath}");
    }

    public Task DeleteFileAsync(string filePath)
    {
        var basePath = _configuration["Storage:BasePath"];
        var fullPath = Path.Combine(basePath, filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {Path}", fullPath);
        }

        return Task.CompletedTask;
    }
}
```

**Konfiguracja w appsettings.Development.json:**

```json
{
  "Storage": {
    "BasePath": "E:\\PortalForge\\Storage",
    "BaseUrl": "https://localhost:5001/files"
  }
}
```

**Zgodność z CLAUDE.md:**

- ✅ **NIGDY nie hardcodować ścieżek** - użyć IConfiguration
- ✅ Structured logging
- ✅ Exception handling dla brakującej konfiguracji

---

## 📋 FAZA 3: Background Jobs

**Infrastruktura:** Hangfire (już używany w projekcie)

### 3.1 `UpdateVacationAllowancesJob.cs`

**Plik:** `backend/PortalForge.Infrastructure/BackgroundJobs/UpdateVacationAllowancesJob.cs`

**Harmonogram:** 1 stycznia każdego roku, 00:00

```csharp
public class UpdateVacationAllowancesJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateVacationAllowancesJob> _logger;

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting annual vacation allowance update");

        var activeUsers = await _unitOfWork.UserRepository.GetAllActiveAsync();

        foreach (var user in activeUsers)
        {
            // Przenieś niewykorzystane dni do CarriedOver
            var unusedDays = user.AnnualVacationDays - user.VacationDaysUsed;
            user.CarriedOverVacationDays = Math.Max(0, unusedDays);
            user.CarriedOverExpiryDate = new DateTime(DateTime.UtcNow.Year, 9, 30);

            // Resetuj pule
            user.AnnualVacationDays = 26; // lub z umowy
            user.VacationDaysUsed = 0;
            user.OnDemandVacationDaysUsed = 0;
            user.CircumstantialLeaveDaysUsed = 0;

            await _unitOfWork.UserRepository.UpdateAsync(user);

            _logger.LogInformation(
                "Updated vacation for user {UserId}: Carried over {Days} days",
                user.Id, user.CarriedOverVacationDays);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Annual vacation allowance update completed");
    }
}
```

**Rejestracja w Hangfire:**

```csharp
// Startup.cs lub Program.cs
RecurringJob.AddOrUpdate<UpdateVacationAllowancesJob>(
    "update-vacation-allowances",
    job => job.ExecuteAsync(),
    "0 0 1 1 *"); // 1 stycznia, 00:00
```

**Zgodność z CLAUDE.md:**

- ✅ Async/await
- ✅ Structured logging
- ✅ Repository pattern

---

### 3.2 `ExpireCarriedOverVacationJob.cs`

**Plik:** `backend/PortalForge.Infrastructure/BackgroundJobs/ExpireCarriedOverVacationJob.cs`

**Harmonogram:** 30 września każdego roku, 23:59

```csharp
public class ExpireCarriedOverVacationJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ExpireCarriedOverVacationJob> _logger;

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting carried over vacation expiration");

        var usersWithCarriedOver = await _unitOfWork.UserRepository
            .GetUsersWithCarriedOverVacationAsync();

        foreach (var user in usersWithCarriedOver)
        {
            if (user.CarriedOverVacationDays > 0)
            {
                _logger.LogInformation(
                    "Expiring {Days} carried over vacation days for user {UserId}",
                    user.CarriedOverVacationDays, user.Id);

                user.CarriedOverVacationDays = 0;
                user.CarriedOverExpiryDate = null;

                await _unitOfWork.UserRepository.UpdateAsync(user);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Carried over vacation expiration completed");
    }
}
```

**Rejestracja:**

```csharp
RecurringJob.AddOrUpdate<ExpireCarriedOverVacationJob>(
    "expire-carried-over-vacation",
    job => job.ExecuteAsync(),
    "59 23 30 9 *"); // 30 września, 23:59
```

---

### 3.3 `SendCarriedOverVacationRemindersJob.cs`

**Plik:** `backend/PortalForge.Infrastructure/BackgroundJobs/SendCarriedOverVacationRemindersJob.cs`

**Harmonogram:** 1 września każdego roku

```csharp
public class SendCarriedOverVacationRemindersJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public async Task ExecuteAsync()
    {
        var usersWithCarriedOver = await _unitOfWork.UserRepository
            .GetUsersWithCarriedOverVacationAsync();

        foreach (var user in usersWithCarriedOver)
        {
            if (user.CarriedOverVacationDays > 0)
            {
                await _notificationService.CreateNotificationAsync(
                    user.Id,
                    NotificationType.CarriedOverVacationExpiring,
                    "Urlop zaległy wygasa za miesiąc",
                    $"Masz {user.CarriedOverVacationDays} dni urlopu z poprzedniego roku. " +
                    $"Wykorzystaj je do 30 września, inaczej przepadną.",
                    null, null,
                    "/dashboard/account");
            }
        }
    }
}
```

---

### 3.4 `CheckApprovalDeadlinesJob.cs`

**Plik:** `backend/PortalForge.Infrastructure/BackgroundJobs/CheckApprovalDeadlinesJob.cs`

**Harmonogram:** Codziennie, 09:00

```csharp
public class CheckApprovalDeadlinesJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CheckApprovalDeadlinesJob> _logger;

    public async Task ExecuteAsync()
    {
        var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

        var overdueSteps = await _unitOfWork.RequestRepository
            .GetApprovalStepsInReviewSinceAsync(threeDaysAgo);

        foreach (var step in overdueSteps)
        {
            _logger.LogWarning(
                "Approval step {StepId} for request {RequestId} is overdue",
                step.Id, step.RequestId);

            await _notificationService.NotifyApprovalDeadlineAsync(
                step.RequestId,
                step.ApproverId);
        }
    }
}
```

**Rejestracja:**

```csharp
RecurringJob.AddOrUpdate<CheckApprovalDeadlinesJob>(
    "check-approval-deadlines",
    job => job.ExecuteAsync(),
    "0 9 * * *"); // Codziennie o 9:00
```

---

### 3.5 Aktualizacja `UpdateVacationStatusesJob.cs`

**Plik:** `backend/PortalForge.Infrastructure/BackgroundJobs/UpdateVacationStatusesJob.cs` (istniejący)

**Rozszerzenie:** Dodać tworzenie `SickLeave` po zatwierdzeniu wniosku L4

```csharp
// W istniejącym job dodaj:
public async Task ProcessApprovedSickLeaveRequestsAsync()
{
    var approvedSickLeaveRequests = await _unitOfWork.RequestRepository
        .GetApprovedSickLeaveRequestsWithoutSickLeaveAsync();

    foreach (var request in approvedSickLeaveRequests)
    {
        var startDate = /* parse from FormData */;
        var endDate = /* parse from FormData */;
        var daysCount = (endDate - startDate).Days + 1;

        var sickLeave = new SickLeave
        {
            Id = Guid.NewGuid(),
            UserId = request.SubmittedById,
            StartDate = startDate,
            EndDate = endDate,
            DaysCount = daysCount,
            SourceRequestId = request.Id,
            Status = SickLeaveStatus.Active,
            RequiresZusDocument = daysCount > 33,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SickLeaveRepository.CreateAsync(sickLeave);

        // Przypomnienie o ZUS jeśli > 33 dni
        if (sickLeave.RequiresZusDocument)
        {
            await _notificationService.CreateNotificationAsync(
                request.SubmittedById,
                NotificationType.System,
                "Wymagane zaświadczenie ZUS",
                "Twoje zwolnienie lekarskie przekracza 33 dni. Wymagane jest dostarczenie zaświadczenia ZUS.",
                "SickLeave",
                sickLeave.Id.ToString(),
                $"/dashboard/sick-leave/{sickLeave.Id}");
        }
    }

    await _unitOfWork.SaveChangesAsync();
}
```

**Zgodność z CLAUDE.md:**

- ✅ Background jobs dla automatyzacji
- ✅ Powiadomienia użytkowników
- ✅ Repository pattern

---

## 📋 FAZA 4: API Endpoints (Commands & Queries)

### 4.1 Vacation Management

#### **GetUserVacationSummaryQuery**

**Plik:** `backend/PortalForge.Application/UseCases/Users/Queries/GetUserVacationSummary/GetUserVacationSummaryQuery.cs`

```csharp
public class GetUserVacationSummaryQuery : IRequest<VacationSummaryDto>
{
    public Guid UserId { get; set; }
}

public class VacationSummaryDto
{
    public int AnnualVacationDays { get; set; }
    public int VacationDaysUsed { get; set; }
    public int VacationDaysRemaining { get; set; }
    public int OnDemandVacationDaysUsed { get; set; }
    public int OnDemandVacationDaysRemaining { get; set; }
    public int CircumstantialLeaveDaysUsed { get; set; }
    public int CarriedOverVacationDays { get; set; }
    public DateTime? CarriedOverExpiryDate { get; set; }
    public int TotalAvailableVacationDays { get; set; }
}

public class GetUserVacationSummaryQueryHandler
    : IRequestHandler<GetUserVacationSummaryQuery, VacationSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<VacationSummaryDto> Handle(
        GetUserVacationSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User {request.UserId} not found");

        return new VacationSummaryDto
        {
            AnnualVacationDays = user.AnnualVacationDays,
            VacationDaysUsed = user.VacationDaysUsed,
            VacationDaysRemaining = user.AnnualVacationDays - user.VacationDaysUsed,
            OnDemandVacationDaysUsed = user.OnDemandVacationDaysUsed,
            OnDemandVacationDaysRemaining = 4 - user.OnDemandVacationDaysUsed,
            CircumstantialLeaveDaysUsed = user.CircumstantialLeaveDaysUsed,
            CarriedOverVacationDays = user.CarriedOverVacationDays,
            CarriedOverExpiryDate = user.CarriedOverExpiryDate,
            TotalAvailableVacationDays = user.TotalAvailableVacationDays
        };
    }
}
```

**Controller:**

```csharp
[HttpGet("users/{userId:guid}/vacation-summary")]
public async Task<ActionResult<VacationSummaryDto>> GetVacationSummary(Guid userId)
{
    var query = new GetUserVacationSummaryQuery { UserId = userId };
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**Zgodność z CLAUDE.md:**

- ✅ CQRS pattern (Query)
- ✅ MediatR
- ✅ DTO dla response
- ✅ NotFoundException

---

#### **UpdateUserVacationAllowanceCommand**

**Plik:** `backend/PortalForge.Application/UseCases/Users/Commands/UpdateVacationAllowance/UpdateUserVacationAllowanceCommand.cs`

```csharp
public class UpdateUserVacationAllowanceCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid UserId { get; set; }
    public int NewAnnualDays { get; set; }
    public string Reason { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string? IpAddress { get; set; }
}

public class UpdateUserVacationAllowanceCommandValidator
    : AbstractValidator<UpdateUserVacationAllowanceCommand>
{
    public UpdateUserVacationAllowanceCommandValidator()
    {
        RuleFor(x => x.NewAnnualDays)
            .GreaterThan(0).WithMessage("Liczba dni urlopu musi być większa niż 0")
            .LessThanOrEqualTo(50).WithMessage("Liczba dni urlopu nie może przekraczać 50");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Powód zmiany jest wymagany")
            .MaximumLength(500).WithMessage("Powód nie może przekraczać 500 znaków");
    }
}

public class UpdateUserVacationAllowanceCommandHandler
    : IRequestHandler<UpdateUserVacationAllowanceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UpdateUserVacationAllowanceCommandHandler> _logger;

    public async Task<Unit> Handle(
        UpdateUserVacationAllowanceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Walidacja
        await _validatorService.ValidateAsync(request);

        // 2. Pobierz użytkownika
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User {request.UserId} not found");

        // 3. Zapisz starą wartość
        var oldValue = user.AnnualVacationDays;

        // 4. Aktualizuj
        user.AnnualVacationDays = request.NewAnnualDays;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        // 5. Audit log
        await _auditLogService.LogActionAsync(
            entityType: "User",
            entityId: user.Id.ToString(),
            action: "VacationAllowanceUpdated",
            userId: request.RequestedByUserId,
            oldValue: oldValue.ToString(),
            newValue: request.NewAnnualDays.ToString(),
            reason: request.Reason,
            ipAddress: request.IpAddress);

        // 6. Powiadomienie dla użytkownika
        await _notificationService.CreateNotificationAsync(
            user.Id,
            NotificationType.VacationAllowanceUpdated,
            "Zmiana limitu urlopów",
            $"Twój limit urlopów został zmieniony z {oldValue} na {request.NewAnnualDays} dni. Powód: {request.Reason}",
            "User",
            user.Id.ToString(),
            "/dashboard/account");

        _logger.LogInformation(
            "Vacation allowance updated for user {UserId} from {OldValue} to {NewValue} by {RequestedBy}",
            user.Id, oldValue, request.NewAnnualDays, request.RequestedByUserId);

        return Unit.Value;
    }
}
```

**Authorization Policy:**

```csharp
// W AuthorizationPolicies.cs
public static class Policies
{
    public const string CanManageVacationAllowance = "CanManageVacationAllowance";
}

// W Startup/Program.cs
services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CanManageVacationAllowance, policy =>
        policy.RequireRole("Admin", "HR", "Manager")); // Manager tylko dla swoich podwładnych - sprawdzane w handler
});
```

**Controller:**

```csharp
[HttpPut("users/{userId:guid}/vacation-allowance")]
[Authorize(Policy = Policies.CanManageVacationAllowance)]
public async Task<IActionResult> UpdateVacationAllowance(
    Guid userId,
    [FromBody] UpdateVacationAllowanceRequest request)
{
    var command = new UpdateUserVacationAllowanceCommand
    {
        UserId = userId,
        NewAnnualDays = request.NewAnnualDays,
        Reason = request.Reason,
        RequestedByUserId = User.GetUserId(),
        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
    };

    await _mediator.Send(command);
    return NoContent();
}
```

**Zgodność z CLAUDE.md:**

- ✅ CQRS + MediatR
- ✅ FluentValidation
- ✅ ITransactionalRequest (auto transaction)
- ✅ Audit log
- ✅ Powiadomienia
- ✅ Structured logging
- ✅ Authorization policies

---

### 4.2 Request Management

#### **SubmitRequestCommand** (rozszerzenie istniejącego)

**Plik:** `backend/PortalForge.Application/UseCases/Requests/Commands/SubmitRequest/SubmitRequestCommand.cs`

**Rozszerzenia:**

```csharp
public class SubmitRequestCommand : IRequest<Guid>, ITransactionalRequest
{
    // Istniejące pola...
    public Guid RequestTemplateId { get; set; }
    public string FormData { get; set; }

    // NOWE POLA
    public LeaveType? LeaveType { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

// W handler:
public async Task<Guid> Handle(SubmitRequestCommand request, CancellationToken cancellationToken)
{
    // 1. Walidacja
    await _validatorService.ValidateAsync(request);

    var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(request.RequestTemplateId)
        ?? throw new NotFoundException($"Template {request.RequestTemplateId} not found");

    // 2. NOWE: Walidacja struktury organizacyjnej
    var (isValid, errors) = await _requestRoutingService.ValidateApprovalStructureAsync(
        request.SubmittedById,
        template.ApprovalStepTemplates);

    if (!isValid)
    {
        throw new ValidationCustomException(
            "Nie możesz złożyć tego wniosku z powodu braku wymaganej struktury organizacyjnej",
            errors);
    }

    // 3. NOWE: Walidacja retrospektywności
    if (template.MaxRetrospectiveDays.HasValue)
    {
        var startDate = /* parse from FormData */;
        var maxAllowedDate = DateTime.UtcNow.AddDays(-template.MaxRetrospectiveDays.Value);

        if (startDate < maxAllowedDate)
        {
            throw new ValidationCustomException(
                $"Nie możesz złożyć wniosku na datę wsteczną starszą niż {template.MaxRetrospectiveDays} dni");
        }
    }

    // 4. NOWE: Walidacja dostępności urlopu
    if (template.IsVacationRequest && request.LeaveType.HasValue)
    {
        var startDate = /* parse from FormData */;
        var endDate = /* parse from FormData */;

        var (canTake, errorMessage) = await _vacationCalculationService.CanTakeVacationAsync(
            request.SubmittedById,
            startDate,
            endDate,
            request.LeaveType.Value);

        if (!canTake)
        {
            throw new ValidationCustomException(errorMessage);
        }
    }

    // 5. NOWE: Upload załączników
    string? attachmentsJson = null;
    if (request.Attachments?.Any() == true && template.AllowsAttachments)
    {
        var uploadedFiles = new List<string>();

        foreach (var file in request.Attachments)
        {
            var filePath = await _fileStorageService.SaveFileAsync(
                file.OpenReadStream(),
                file.FileName,
                "request-attachments");

            uploadedFiles.Add(filePath);
        }

        attachmentsJson = JsonSerializer.Serialize(uploadedFiles);
    }

    // 6. Utwórz request
    var newRequest = new Request
    {
        Id = Guid.NewGuid(),
        RequestNumber = await GenerateRequestNumberAsync(),
        RequestTemplateId = request.RequestTemplateId,
        SubmittedById = request.SubmittedById,
        SubmittedAt = DateTime.UtcNow,
        FormData = request.FormData,
        Status = RequestStatus.InReview,
        Priority = request.Priority,
        LeaveType = request.LeaveType,
        Attachments = attachmentsJson
    };

    await _unitOfWork.RequestRepository.CreateAsync(newRequest);

    // 7. Routing approval steps (istniejąca logika + zastępcy)
    await CreateApprovalStepsWithSubstitutesAsync(newRequest, template);

    // 8. NOWE: Auto-approve dla L4 i urlopu na żądanie
    if (template.IsSickLeaveRequest)
    {
        // L4 - auto-approve pierwszy krok
        var firstStep = newRequest.ApprovalSteps.First();
        firstStep.Status = ApprovalStepStatus.Approved;
        firstStep.FinishedAt = DateTime.UtcNow;

        // Powiadom przełożonego (do wiadomości)
        await _notificationService.CreateNotificationAsync(
            firstStep.ApproverId,
            NotificationType.SickLeaveSubmitted,
            "Pracownik zgłosił L4",
            $"{submitter.FirstName} {submitter.LastName} zgłosił zwolnienie lekarskie od {startDate:dd.MM.yyyy} do {endDate:dd.MM.yyyy}",
            "Request",
            newRequest.Id.ToString(),
            $"/dashboard/requests/{newRequest.Id}");
    }
    else if (request.LeaveType == LeaveType.OnDemand)
    {
        // Urlop na żądanie - auto-approve
        var firstStep = newRequest.ApprovalSteps.First();
        firstStep.Status = ApprovalStepStatus.Approved;
        firstStep.FinishedAt = DateTime.UtcNow;

        // Natychmiast utwórz VacationSchedule
        await CreateVacationScheduleAsync(newRequest);
    }
    else
    {
        // Standardowy flow - powiadom pierwszego approvera
        await _notificationService.NotifyApproverAsync(/* ... */);
    }

    return newRequest.Id;
}

private async Task CreateApprovalStepsWithSubstitutesAsync(Request request, RequestTemplate template)
{
    foreach (var stepTemplate in template.ApprovalStepTemplates.OrderBy(s => s.StepOrder))
    {
        var approverId = await _requestRoutingService.GetApproverForStepWithSubstituteAsync(
            request.SubmittedById,
            stepTemplate);

        if (approverId == null)
        {
            throw new ValidationCustomException(
                "Nie można utworzyć wniosku - brak dostępnego przełożonego (również zastępcy)");
        }

        var step = new RequestApprovalStep
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            StepOrder = stepTemplate.StepOrder,
            ApproverId = approverId.Value,
            Status = stepTemplate.StepOrder == 1
                ? ApprovalStepStatus.InReview
                : ApprovalStepStatus.Pending,
            RequiresQuiz = stepTemplate.RequiresQuiz
        };

        request.ApprovalSteps.Add(step);
    }
}
```

**Zgodność z CLAUDE.md:**

- ✅ Guard clauses na początku
- ✅ Walidacja biznesowa przed zapisem
- ✅ Early returns dla error cases
- ✅ Happy path na końcu
- ✅ FluentValidation
- ✅ Exception handling z custom exceptions

---

#### **EditRequestCommand** (nowe)

**Plik:** `backend/PortalForge.Application/UseCases/Requests/Commands/EditRequest/EditRequestCommand.cs`

```csharp
public class EditRequestCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid RequestId { get; set; }
    public Guid EditedByUserId { get; set; }
    public string NewFormData { get; set; }
    public string? ChangeReason { get; set; }
}

public class EditRequestCommandValidator : AbstractValidator<EditRequestCommand>
{
    public EditRequestCommandValidator()
    {
        RuleFor(x => x.NewFormData)
            .NotEmpty().WithMessage("Dane formularza są wymagane");

        RuleFor(x => x.ChangeReason)
            .MaximumLength(500);
    }
}

public class EditRequestCommandHandler : IRequestHandler<EditRequestCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;

    public async Task<Unit> Handle(EditRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Walidacja
        await _validatorService.ValidateAsync(request);

        var existingRequest = await _unitOfWork.RequestRepository.GetByIdAsync(request.RequestId)
            ?? throw new NotFoundException($"Request {request.RequestId} not found");

        // 2. Sprawdź uprawnienia
        if (existingRequest.SubmittedById != request.EditedByUserId)
        {
            throw new ForbiddenException("Możesz edytować tylko własne wnioski");
        }

        // 3. Sprawdź status
        if (existingRequest.Status != RequestStatus.Draft && existingRequest.Status != RequestStatus.InReview)
        {
            throw new ValidationCustomException("Możesz edytować tylko wnioski ze statusem Draft lub InReview");
        }

        // 4. Zapisz historię edycji
        var editHistory = new RequestEditHistory
        {
            Id = Guid.NewGuid(),
            RequestId = request.RequestId,
            EditedByUserId = request.EditedByUserId,
            EditedAt = DateTime.UtcNow,
            OldFormData = existingRequest.FormData,
            NewFormData = request.NewFormData,
            ChangeReason = request.ChangeReason
        };

        await _unitOfWork.RequestEditHistoryRepository.CreateAsync(editHistory);

        // 5. Zaktualizuj request
        existingRequest.FormData = request.NewFormData;
        await _unitOfWork.RequestRepository.UpdateAsync(existingRequest);

        // 6. Powiadom approverów którzy już zaakceptowali/odrzucili
        var notifiedApprovers = existingRequest.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Approved || s.Status == ApprovalStepStatus.Rejected)
            .Select(s => s.ApproverId)
            .Distinct();

        foreach (var approverId in notifiedApprovers)
        {
            await _notificationService.CreateNotificationAsync(
                approverId,
                NotificationType.RequestEdited,
                "Wniosek został edytowany",
                $"Wniosek {existingRequest.RequestNumber} został edytowany przez składającego",
                "Request",
                request.RequestId.ToString(),
                $"/dashboard/requests/{request.RequestId}");
        }

        return Unit.Value;
    }
}
```

**Controller:**

```csharp
[HttpPut("requests/{requestId:guid}")]
[Authorize]
public async Task<IActionResult> EditRequest(
    Guid requestId,
    [FromBody] EditRequestRequest request)
{
    var command = new EditRequestCommand
    {
        RequestId = requestId,
        EditedByUserId = User.GetUserId(),
        NewFormData = request.FormData,
        ChangeReason = request.ChangeReason
    };

    await _mediator.Send(command);
    return NoContent();
}
```

---

#### **CancelVacationCommand** (nowe)

**Plik:** `backend/PortalForge.Application/UseCases/Vacations/Commands/CancelVacation/CancelVacationCommand.cs`

```csharp
public class CancelVacationCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid VacationScheduleId { get; set; }
    public Guid CancelledByUserId { get; set; }
    public string Reason { get; set; }
}

public class CancelVacationCommandHandler : IRequestHandler<CancelVacationCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;

    public async Task<Unit> Handle(CancelVacationCommand request, CancellationToken cancellationToken)
    {
        var vacation = await _unitOfWork.VacationScheduleRepository.GetByIdAsync(request.VacationScheduleId)
            ?? throw new NotFoundException($"Vacation {request.VacationScheduleId} not found");

        var cancelledBy = await _unitOfWork.UserRepository.GetByIdAsync(request.CancelledByUserId);
        var employee = await _unitOfWork.UserRepository.GetByIdAsync(vacation.UserId);

        // 1. Sprawdź uprawnienia
        var isAdmin = cancelledBy.Role == UserRole.Admin;
        var isApprover = vacation.SourceRequest.ApprovalSteps
            .Any(s => s.ApproverId == request.CancelledByUserId && s.Status == ApprovalStepStatus.Approved);

        var daysSinceStart = (DateTime.UtcNow - vacation.StartDate).Days;

        if (!isAdmin)
        {
            if (!isApprover)
            {
                throw new ForbiddenException("Nie masz uprawnień do anulowania tego urlopu");
            }

            if (daysSinceStart > 1)
            {
                throw new ValidationCustomException(
                    "Przełożony może anulować urlop tylko do 1 dnia po jego rozpoczęciu. Skontaktuj się z administratorem.");
            }
        }

        // 2. Anuluj urlop
        vacation.Status = VacationStatus.Cancelled;
        await _unitOfWork.VacationScheduleRepository.UpdateAsync(vacation);

        // 3. Zwróć dni do puli użytkownika
        employee.VacationDaysUsed -= vacation.DaysCount;
        await _unitOfWork.UserRepository.UpdateAsync(employee);

        // 4. Audit log
        await _auditLogService.LogActionAsync(
            "VacationSchedule",
            vacation.Id.ToString(),
            "VacationCancelled",
            request.CancelledByUserId,
            vacation.Status.ToString(),
            "Cancelled",
            request.Reason);

        // 5. Powiadomienia
        await _notificationService.CreateNotificationAsync(
            vacation.UserId,
            NotificationType.VacationCancelled,
            "Urlop został anulowany",
            $"Twój urlop od {vacation.StartDate:dd.MM.yyyy} do {vacation.EndDate:dd.MM.yyyy} został anulowany. Powód: {request.Reason}",
            "VacationSchedule",
            vacation.Id.ToString(),
            "/dashboard/account");

        if (vacation.SubstituteUserId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                vacation.SubstituteUserId.Value,
                NotificationType.System,
                "Urlop został anulowany",
                $"Urlop {employee.FirstName} {employee.LastName}, dla którego byłeś/aś zastępcą, został anulowany.",
                null, null, null);
        }

        return Unit.Value;
    }
}
```

**Controller:**

```csharp
[HttpDelete("vacation-schedules/{vacationId:guid}")]
[Authorize]
public async Task<IActionResult> CancelVacation(
    Guid vacationId,
    [FromBody] CancelVacationRequest request)
{
    var command = new CancelVacationCommand
    {
        VacationScheduleId = vacationId,
        CancelledByUserId = User.GetUserId(),
        Reason = request.Reason
    };

    await _mediator.Send(command);
    return NoContent();
}
```

**Zgodność z CLAUDE.md:**

- ✅ Guard clauses
- ✅ Authorization checks na początku
- ✅ Business logic validation
- ✅ Audit log
- ✅ Notifications

---

### 4.3 Comments

#### **AddRequestCommentCommand**

**Plik:** `backend/PortalForge.Application/UseCases/Requests/Commands/AddComment/AddRequestCommentCommand.cs`

```csharp
public class AddRequestCommentCommand : IRequest<Guid>
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string Comment { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public class AddRequestCommentCommandValidator : AbstractValidator<AddRequestCommentCommand>
{
    public AddRequestCommentCommandValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Komentarz nie może być pusty")
            .MaximumLength(2000).WithMessage("Komentarz nie może przekraczać 2000 znaków");
    }
}

public class AddRequestCommentCommandHandler : IRequestHandler<AddRequestCommentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public async Task<Guid> Handle(AddRequestCommentCommand request, CancellationToken cancellationToken)
    {
        var existingRequest = await _unitOfWork.RequestRepository.GetByIdAsync(request.RequestId)
            ?? throw new NotFoundException($"Request {request.RequestId} not found");

        // Upload załączników
        string? attachmentsJson = null;
        if (request.Attachments?.Any() == true)
        {
            var uploadedFiles = new List<string>();

            foreach (var file in request.Attachments)
            {
                var filePath = await _fileStorageService.SaveFileAsync(
                    file.OpenReadStream(),
                    file.FileName,
                    "request-comments");

                uploadedFiles.Add(filePath);
            }

            attachmentsJson = JsonSerializer.Serialize(uploadedFiles);
        }

        // Utwórz komentarz
        var comment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.RequestId,
            UserId = request.UserId,
            Comment = request.Comment,
            Attachments = attachmentsJson,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RequestCommentRepository.CreateAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        // Powiadomienia
        await _notificationService.NotifyRequestCommentAsync(request.RequestId, request.UserId);

        return comment.Id;
    }
}
```

---

### 4.4 Department Calendar & Reports

#### **GetDepartmentVacationCalendarQuery**

**Plik:** `backend/PortalForge.Application/UseCases/Departments/Queries/GetDepartmentVacationCalendar/GetDepartmentVacationCalendarQuery.cs`

```csharp
public class GetDepartmentVacationCalendarQuery : IRequest<List<VacationCalendarEntryDto>>
{
    public Guid DepartmentId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class VacationCalendarEntryDto
{
    public Guid VacationId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; }
    public string Position { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysCount { get; set; }
    public VacationStatus Status { get; set; }
}

public class GetDepartmentVacationCalendarQueryHandler
    : IRequestHandler<GetDepartmentVacationCalendarQuery, List<VacationCalendarEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<List<VacationCalendarEntryDto>> Handle(
        GetDepartmentVacationCalendarQuery request,
        CancellationToken cancellationToken)
    {
        // Pobierz urlopy wszystkich pracowników działu (włącznie z subdepartmentami)
        var vacations = await _unitOfWork.VacationScheduleRepository
            .GetByDepartmentAndDateRangeAsync(
                request.DepartmentId,
                request.FromDate,
                request.ToDate);

        return vacations.Select(v => new VacationCalendarEntryDto
        {
            VacationId = v.Id,
            UserId = v.UserId,
            UserFullName = $"{v.User.FirstName} {v.User.LastName}",
            Position = v.User.PositionEntity?.Name ?? v.User.Position,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            DaysCount = v.DaysCount,
            Status = v.Status
        }).ToList();
    }
}
```

**Controller:**

```csharp
[HttpGet("departments/{departmentId:guid}/vacation-calendar")]
[Authorize]
public async Task<ActionResult<List<VacationCalendarEntryDto>>> GetVacationCalendar(
    Guid departmentId,
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null)
{
    var query = new GetDepartmentVacationCalendarQuery
    {
        DepartmentId = departmentId,
        FromDate = from ?? DateTime.UtcNow.Date,
        ToDate = to ?? DateTime.UtcNow.AddMonths(3).Date
    };

    var result = await _mediator.Send(query);
    return Ok(result);
}
```

---

#### **ExportDepartmentVacationReportQuery**

**Plik:** `backend/PortalForge.Application/UseCases/Departments/Queries/ExportVacationReport/ExportDepartmentVacationReportQuery.cs`

```csharp
public class ExportDepartmentVacationReportQuery : IRequest<byte[]>
{
    public Guid DepartmentId { get; set; }
    public int Year { get; set; }
    public ReportFormat Format { get; set; } // PDF or Excel
}

public enum ReportFormat { PDF, Excel }

public class ExportDepartmentVacationReportQueryHandler
    : IRequestHandler<ExportDepartmentVacationReportQuery, byte[]>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReportGenerator _reportGenerator;

    public async Task<byte[]> Handle(
        ExportDepartmentVacationReportQuery request,
        CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId)
            ?? throw new NotFoundException($"Department {request.DepartmentId} not found");

        var vacations = await _unitOfWork.VacationScheduleRepository
            .GetByDepartmentAndYearAsync(request.DepartmentId, request.Year);

        var reportData = new VacationReportData
        {
            DepartmentName = department.Name,
            Year = request.Year,
            Vacations = vacations.Select(v => new VacationReportEntry
            {
                EmployeeName = $"{v.User.FirstName} {v.User.LastName}",
                Position = v.User.PositionEntity?.Name,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                DaysCount = v.DaysCount,
                Status = v.Status.ToString()
            }).ToList()
        };

        return request.Format == ReportFormat.PDF
            ? await _reportGenerator.GeneratePdfAsync(reportData)
            : await _reportGenerator.GenerateExcelAsync(reportData);
    }
}
```

**Implementacja IReportGenerator należy do FAZY Infrastructure.**

---

### 4.5 Audit Logs

#### **GetAuditLogsQuery**

**Plik:** `backend/PortalForge.Application/UseCases/Admin/Queries/GetAuditLogs/GetAuditLogsQuery.cs`

```csharp
public class GetAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
{
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; }
    public Guid? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _unitOfWork.AuditLogRepository.GetPagedAsync(
            entityType: request.EntityType,
            action: request.Action,
            userId: request.UserId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            page: request.Page,
            pageSize: request.PageSize);

        return new PagedResult<AuditLogDto>
        {
            Items = logs.Items.Select(l => new AuditLogDto
            {
                Id = l.Id,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Action = l.Action,
                UserId = l.UserId,
                UserFullName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : null,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                Reason = l.Reason,
                Timestamp = l.Timestamp,
                IpAddress = l.IpAddress
            }).ToList(),
            TotalCount = logs.TotalCount,
            Page = logs.Page,
            PageSize = logs.PageSize
        };
    }
}
```

**Controller:**

```csharp
[HttpGet("admin/audit-logs")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
    [FromQuery] GetAuditLogsQuery query)
{
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

---

### 4.6 Transfer Employee

#### **TransferEmployeeToDepartmentCommand**

**Plik:** `backend/PortalForge.Application/UseCases/Users/Commands/TransferDepartment/TransferEmployeeToDepartmentCommand.cs`

```csharp
public class TransferEmployeeToDepartmentCommand : IRequest<Unit>, ITransactionalRequest
{
    public Guid UserId { get; set; }
    public Guid NewDepartmentId { get; set; }
    public Guid? NewSupervisorId { get; set; }
    public Guid TransferredByUserId { get; set; }
    public string? Reason { get; set; }
}

public class TransferEmployeeToDepartmentCommandHandler
    : IRequestHandler<TransferEmployeeToDepartmentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public async Task<Unit> Handle(
        TransferEmployeeToDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User {request.UserId} not found");

        var newDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.NewDepartmentId)
            ?? throw new NotFoundException($"Department {request.NewDepartmentId} not found");

        var oldDepartmentId = user.DepartmentId;
        var oldSupervisorId = user.SupervisorId;

        // 1. Zaktualizuj użytkownika
        user.DepartmentId = request.NewDepartmentId;
        user.SupervisorId = request.NewSupervisorId;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        // 2. Przepnij pending requests na nowego przełożonego
        if (request.NewSupervisorId.HasValue)
        {
            var pendingRequests = await _unitOfWork.RequestRepository
                .GetPendingRequestsByUserAsync(request.UserId);

            foreach (var req in pendingRequests)
            {
                var pendingSteps = req.ApprovalSteps
                    .Where(s => s.Status == ApprovalStepStatus.InReview || s.Status == ApprovalStepStatus.Pending);

                foreach (var step in pendingSteps)
                {
                    if (step.ApproverId == oldSupervisorId)
                    {
                        step.ApproverId = request.NewSupervisorId.Value;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // 3. Audit log
        await _auditLogService.LogActionAsync(
            "User",
            user.Id.ToString(),
            "DepartmentTransfer",
            request.TransferredByUserId,
            $"Department: {oldDepartmentId}, Supervisor: {oldSupervisorId}",
            $"Department: {request.NewDepartmentId}, Supervisor: {request.NewSupervisorId}",
            request.Reason);

        // 4. Powiadomienia
        if (oldSupervisorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                oldSupervisorId.Value,
                NotificationType.System,
                "Pracownik przeniesiony",
                $"{user.FirstName} {user.LastName} został przeniesiony do innego działu",
                null, null, null);
        }

        if (request.NewSupervisorId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                request.NewSupervisorId.Value,
                NotificationType.System,
                "Nowy pracownik",
                $"{user.FirstName} {user.LastName} został przeniesiony do Twojego działu",
                "User",
                user.Id.ToString(),
                $"/dashboard/users/{user.Id}");
        }

        return Unit.Value;
    }
}
```

**Zgodność z CLAUDE.md:**

- ✅ CQRS + MediatR
- ✅ ITransactionalRequest
- ✅ Audit log
- ✅ Powiadomienia dla wszystkich zaangażowanych
- ✅ Przepięcie pending requests

---

## 📋 FAZA 5-9: Frontend (szczegółowo w osobnych plikach implementacyjnych)

Ze względu na rozmiar tego dokumentu, szczegóły implementacji frontendu (strony, komponenty, composables) będą zawarte w osobnych plikach:

- **FAZA 5:** `frontend-account-profile.md`
- **FAZA 6:** `frontend-request-system.md`
- **FAZA 7:** `frontend-vacation-calendar.md`
- **FAZA 8:** `frontend-admin-panel.md`
- **FAZA 9:** `frontend-vacation-management.md`

Kluczowe zasady zgodności z CLAUDE.md dla frontendu:

### Vue 3 Composition API

- ✅ Używać `<script setup lang="ts">`
- ✅ TypeScript interfaces dla wszystkich props/emits
- ✅ Composables dla reusable logic
- ✅ Pinia stores dla global state
- ✅ VueUse dla common utilities

### TypeScript

- ✅ NIGDY nie używać `any`
- ✅ Zdefiniowane types dla API responses
- ✅ Typed props i emits

### Tailwind CSS

- ✅ Utility classes bezpośrednio w template
- ✅ Responsive prefixes (sm:, md:, lg:)
- ✅ Dark mode support (dark:)

### DOM Manipulation

- ✅ **NIGDY nie hardcodować wartości** dla layoutu
- ✅ Używać template refs do dostępu do DOM
- ✅ Obliczać wymiary matematycznie na podstawie rzeczywistych elementów
- ✅ Reagować na resize events

### Error Handling

- ✅ Uniwersalny `ErrorModal.vue` dla wszystkich błędów z API
- ✅ Wyświetlanie `message` i `errors[]` z backend

### API Calls

- ✅ Używać `$fetch` (Nuxt)
- ✅ Proper error handling (try-catch)
- ✅ Loading states
- ✅ TypeScript types dla responses

---

## 📋 FAZA 10: Seedery - Domyślne szablony wniosków

### DefaultRequestTemplatesSeeder.cs

**Plik:** `backend/PortalForge.Infrastructure/Data/Seeders/DefaultRequestTemplatesSeeder.cs`

```csharp
public class DefaultRequestTemplatesSeeder
{
    private readonly ApplicationDbContext _context;

    public async Task SeedAsync()
    {
        if (_context.RequestTemplates.Any(t => t.Name == "Wniosek urlopowy"))
            return; // Already seeded

        var templates = new List<RequestTemplate>
        {
            CreateVacationRequestTemplate(),
            CreateSickLeaveTemplate()
        };

        await _context.RequestTemplates.AddRangeAsync(templates);
        await _context.SaveChangesAsync();
    }

    private RequestTemplate CreateVacationRequestTemplate()
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Wniosek urlopowy",
            Description = "Standardowy wniosek o urlop wypoczynkowy, na żądanie lub okolicznościowy",
            Icon = "calendar",
            Category = "Urlopy i absencje",
            RequiresApproval = true,
            RequiresSubstituteSelection = true,
            AllowsAttachments = false,
            IsVacationRequest = true,
            IsSickLeaveRequest = false,
            MaxRetrospectiveDays = 0, // urlopy nie mogą być wstecz
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>()
        };

        // Pola formularza
        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Typ urlopu",
            FieldType = RequestFieldType.Select,
            IsRequired = true,
            Options = JsonSerializer.Serialize(new[]
            {
                "Urlop wypoczynkowy",
                "Urlop na żądanie",
                "Urlop okolicznościowy"
            }),
            Order = 1,
            HelpText = "Urlop okolicznościowy: ślub, pogrzeb, narodziny dziecka (2 dni)"
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data rozpoczęcia",
            FieldType = RequestFieldType.Date,
            IsRequired = true,
            Order = 2
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data zakończenia",
            FieldType = RequestFieldType.Date,
            IsRequired = true,
            Order = 3
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Powód (dla urlopu okolicznościowego)",
            FieldType = RequestFieldType.Textarea,
            IsRequired = false,
            Order = 4,
            HelpText = "Wymagane dla urlopu okolicznościowego"
        });

        // Approval flow: tylko bezpośredni przełożony
        template.ApprovalStepTemplates.Add(new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            IsRequired = true,
            RequiresQuiz = false
        });

        return template;
    }

    private RequestTemplate CreateSickLeaveTemplate()
    {
        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Zgłoszenie L4 (zwolnienie lekarskie)",
            Description = "Zgłoszenie zwolnienia lekarskiego - automatycznie zatwierdzone, do wiadomości przełożonego",
            Icon = "medical-bag",
            Category = "Urlopy i absencje",
            RequiresApproval = true, // Do wiadomości, ale auto-approve
            RequiresSubstituteSelection = false,
            AllowsAttachments = true, // ZUS po 33 dniach
            IsVacationRequest = false,
            IsSickLeaveRequest = true,
            MaxRetrospectiveDays = 14, // można zgłosić do 14 dni wstecz
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Fields = new List<RequestTemplateField>(),
            ApprovalStepTemplates = new List<RequestApprovalStepTemplate>()
        };

        // Pola formularza
        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data rozpoczęcia zwolnienia",
            FieldType = RequestFieldType.Date,
            IsRequired = true,
            Order = 1,
            HelpText = "Możesz zgłosić zwolnienie do 14 dni wstecz"
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Data zakończenia zwolnienia",
            FieldType = RequestFieldType.Date,
            IsRequired = true,
            Order = 2
        });

        template.Fields.Add(new RequestTemplateField
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            Label = "Uwagi",
            FieldType = RequestFieldType.Textarea,
            IsRequired = false,
            Order = 3
        });

        // Approval flow: bezpośredni przełożony (ale auto-approve w kodzie)
        template.ApprovalStepTemplates.Add(new RequestApprovalStepTemplate
        {
            Id = Guid.NewGuid(),
            RequestTemplateId = template.Id,
            StepOrder = 1,
            ApproverType = ApproverType.DirectSupervisor,
            IsRequired = true,
            RequiresQuiz = false
        });

        return template;
    }
}
```

**Wywołanie seedera w Program.cs:**

```csharp
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DefaultRequestTemplatesSeeder>();
    await seeder.SeedAsync();
}
```

**Zgodność z CLAUDE.md:**

- ✅ Seeder pattern dla initial data
- ✅ Idempotent (sprawdzanie czy już exists)
- ✅ Structured data (JSON dla Options)

---

## 📋 FAZA 11: Walidacje i logika biznesowa

Większość walidacji i logiki biznesowej została już opisana w poprzednich fazach:

### Zaimplementowane walidacje:

1. ✅ **Proporcjonalne naliczanie urlopów** - `VacationCalculationService.CalculateProportionalVacationDays()`
2. ✅ **Walidacja dostępności urlopu** - `VacationCalculationService.CanTakeVacationAsync()`
3. ✅ **Walidacja struktury organizacyjnej** - `RequestRoutingService.ValidateApprovalStructureAsync()`
4. ✅ **Zastępcy** - `RequestRoutingService.GetApproverForStepWithSubstituteAsync()`
5. ✅ **Auto-approve L4** - w `SubmitRequestCommand.Handler`
6. ✅ **Auto-approve urlop na żądanie** - w `SubmitRequestCommand.Handler`
7. ✅ **Walidacja retrospektywności** - w `SubmitRequestCommand.Handler`
8. ✅ **Uprawnienia do anulowania urlopu** - w `CancelVacationCommand.Handler`
9. ✅ **FluentValidation** dla wszystkich commands

### Dodatkowe walidatory:

Każdy command/query ma dedykowany validator zgodnie z wzorcem z CLAUDE.md:

```csharp
// Przykład
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Imię jest wymagane")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail).WithMessage("Email już istnieje");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
    {
        var existing = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        return existing == null;
    }
}
```

**Zgodność z CLAUDE.md:**

- ✅ FluentValidation dla wszystkich commands
- ✅ Async validation dla DB checks
- ✅ Custom error messages po polsku
- ✅ MustAsync dla complex validations

---

## 🎯 Podsumowanie i następne kroki

### Co zostało zaplanowane:

- ✅ 9 nowych/rozszerzonych entities
- ✅ 9 migracji EF Core
- ✅ 4 serwisy biznesowe (Vacation, Audit, Routing, File Storage)
- ✅ 5 background jobs (roczne resety, przypomnienia, SLA)
- ✅ ~25 API endpoints (commands & queries)
- ✅ Pełna zgodność z polskim prawem pracy 2025
- ✅ Audit log dla wszystkich ważnych akcji
- ✅ System powiadomień (in-app + email)
- ✅ Seedery dla domyślnych szablonów

### Zgodność z CLAUDE.md:

**Backend:**

- ✅ Clean Architecture (Domain → Application → Infrastructure → Api)
- ✅ CQRS z MediatR
- ✅ Repository Pattern + Unit of Work
- ✅ FluentValidation dla wszystkich commands
- ✅ ITransactionalRequest dla transakcji
- ✅ Structured logging (Serilog)
- ✅ **NIGDY nie hardcodować ścieżek/konfiguracji** - używać IConfiguration lub SystemSettings
- ✅ Async/await everywhere
- ✅ Guard clauses i early returns
- ✅ XML documentation

**Frontend (do implementacji):**

- ✅ Vue 3 Composition API (`<script setup>`)
- ✅ TypeScript (no `any`)
- ✅ Tailwind CSS utilities
- ✅ Composables dla reusable logic
- ✅ Pinia stores
- ✅ **NIGDY nie hardcodować wymiarów** - template refs + obliczenia matematyczne
- ✅ ErrorModal dla wszystkich błędów API

### Podział na Sprinty (zalecany):

**Sprint 1 (2 tygodnie):** FAZA 1-3 (Backend Core)

- Rozszerzenie modeli
- Migracje
- Serwisy biznesowe
- Background jobs

**Sprint 2 (2 tygodnie):** FAZA 4 (Backend API)

- Wszystkie endpoints
- Walidacje
- Seedery

**Sprint 3 (1.5 tygodnia):** FAZA 5-6 (Frontend Base)

- Profil użytkownika
- System wnioskowy (index, details, new)
- Komponenty (Timeline, Comments, Calendar)

**Sprint 4 (1 tydzień):** FAZA 7-9 (Frontend Advanced)

- Kalendarz urlopów
- Panel admina
- Zarządzanie urlopami

**Sprint 5 (0.5 tygodnia):** FAZA 10-11 (Polish & Testing)

- Testy
- Bug fixing
- UX improvements

---

## 📞 Kontakt i wsparcie

W przypadku pytań lub problemów podczas implementacji:

- Sprawdź `.claude/CLAUDE.md` - główne zasady
- Sprawdź `.claude/backend.md` - wzorce backend
- Sprawdź `.claude/frontend.md` - wzorce frontend
- Sprawdź istniejący kod dla przykładów

---

**Powodzenia w implementacji! 🚀**
