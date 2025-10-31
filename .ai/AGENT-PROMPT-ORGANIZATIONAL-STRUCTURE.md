# 🎯 PROMPT DLA AGENTA - Struktura Organizacyjna PortalForge

## 📊 AKTUALNY STAN - CO JUŻ ZROBIONO

✅ **Tasks 1.1-1.5 UKOŃCZONE (5/41 = 12%)**

Agent już zaimplementował:
- ✅ **Task 1.1**: Department Entity (dział z nieograniczoną hierarchią)
- ✅ **Task 1.2**: VacationSchedule Entity (harmonogram urlopów z zastępcami)
- ✅ **Task 1.3**: OrganizationalPermission Entity (uprawnienia widoczności działów)
- ✅ **Task 1.4**: Rozszerzony enum DepartmentRole (7 poziomów: Employee → TeamLead → Manager → Director → VP → President → BoardMember)
- ✅ **Task 1.5**: Rozszerzony enum ApproverType (dodano DirectSupervisor i SpecificDepartment)

**Commit**: `4e0b579 - feat: implement organizational structure and vacation management entities`

---

## 🎯 TWOJE ZADANIE - DOKOŃCZ SPRINT 1

**CEL**: Bez działającej struktury organizacyjnej NIE DA SIĘ poprawnie zaimplementować routingu wniosków!

Musisz dokończyć **Tasks 1.6-1.8** aby można było:
1. Zapisywać działy do bazy danych (EF Core Configurations)
2. Uruchomić migracje (dodać tabele Departments, VacationSchedules, OrganizationalPermissions)
3. Następnie dopiero można zaimplementować routing wniosków i logikę urlopową

---

## 📚 PLIKI DO PRZECZYTANIA PRZED ROZPOCZĘCIEM

### 1. Zasady projektu (OBOWIĄZKOWE!)
```
D:\Projects\PortalForge\.claude\CLAUDE.md - Główne zasady (NIE dodawaj "Generated with Claude Code"!)
D:\Projects\PortalForge\.claude\backend.md - Backend: Clean Architecture + CQRS + MediatR
D:\Projects\PortalForge\.claude\frontend.md - Frontend: Nuxt 3 + Vue 3 + TypeScript
```

### 2. Plan implementacji (TWÓJ GŁÓWNY PRZEWODNIK)
```
D:\Projects\PortalForge\.ai\implementation-plan-organizational-structure.md
```
**UWAGA**: Ten plik zawiera 41 tasków z checkboxami `[ ]` - MUSISZ je odhaczać `[x]` po każdym ukończonym zadaniu!

### 3. Dokumentacja techniczna
```
D:\Projects\PortalForge\.ai\decisions\005-organizational-structure-and-vacation-system.md
D:\Projects\PortalForge\.ai\backend\organizational-structure.md
D:\Projects\PortalForge\.ai\backend\vacation-schedule-system.md
```

---

## 🚀 KROK PO KROKU - CO ROBIĆ TERAZ

### SPRINT 1 - Task 1.6: EF Core Configurations ⭐ CRITICAL

**Czas**: 2 godziny
**Zależności**: Tasks 1.1, 1.2, 1.3 (już zrobione ✅)

#### Pliki do utworzenia:

#### 1️⃣ DepartmentConfiguration.cs

**Plik**: `backend/PortalForge.Infrastructure/Persistence/Configurations/DepartmentConfiguration.cs`

**Kod**:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Department entity.
/// Configures self-referencing hierarchy and relationships.
/// </summary>
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

        builder.Property(d => d.UpdatedAt)
            .IsRequired(false);

        // Self-referencing relationship (Parent-Child)
        builder.HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Department Head relationship
        builder.HasOne(d => d.HeadOfDepartment)
            .WithMany()
            .HasForeignKey(d => d.HeadOfDepartmentId)
            .OnDelete(DeleteBehavior.SetNull); // If head deleted, set to null

        // Indexes for performance
        builder.HasIndex(d => d.ParentDepartmentId);
        builder.HasIndex(d => d.HeadOfDepartmentId);
        builder.HasIndex(d => d.IsActive);
        builder.HasIndex(d => d.Name);
    }
}
```

#### 2️⃣ VacationScheduleConfiguration.cs

**Plik**: `backend/PortalForge.Infrastructure/Persistence/Configurations/VacationScheduleConfiguration.cs`

**Kod**:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for VacationSchedule entity.
/// </summary>
public class VacationScheduleConfiguration : IEntityTypeConfiguration<VacationSchedule>
{
    public void Configure(EntityTypeBuilder<VacationSchedule> builder)
    {
        builder.ToTable("VacationSchedules");
        builder.HasKey(v => v.Id);

        // Properties
        builder.Property(v => v.UserId)
            .IsRequired();

        builder.Property(v => v.SubstituteUserId)
            .IsRequired();

        builder.Property(v => v.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(v => v.EndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(v => v.SourceRequestId)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>() // Store as string in DB
            .HasMaxLength(50);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Substitute)
            .WithMany()
            .HasForeignKey(v => v.SubstituteUserId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete if substitute deleted

        builder.HasOne(v => v.SourceRequest)
            .WithMany()
            .HasForeignKey(v => v.SourceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for querying
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.SubstituteUserId);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.StartDate);
        builder.HasIndex(v => v.EndDate);
        builder.HasIndex(v => new { v.StartDate, v.EndDate }); // Composite for date range queries
    }
}
```

#### 3️⃣ OrganizationalPermissionConfiguration.cs

**Plik**: `backend/PortalForge.Infrastructure/Persistence/Configurations/OrganizationalPermissionConfiguration.cs`

**Kod**:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OrganizationalPermission entity.
/// </summary>
public class OrganizationalPermissionConfiguration : IEntityTypeConfiguration<OrganizationalPermission>
{
    public void Configure(EntityTypeBuilder<OrganizationalPermission> builder)
    {
        builder.ToTable("OrganizationalPermissions");
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.CanViewAllDepartments)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.VisibleDepartmentIds)
            .IsRequired()
            .HasColumnType("jsonb") // PostgreSQL JSONB for efficient JSON queries
            .HasDefaultValue("[]");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<OrganizationalPermission>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique(); // One permission per user
    }
}
```

#### 4️⃣ Zarejestruj konfiguracje w ApplicationDbContext

**Plik**: `backend/PortalForge.Infrastructure/Persistence/ApplicationDbContext.cs`

**Dodaj DbSets**:
```csharp
public DbSet<Department> Departments => Set<Department>();
public DbSet<VacationSchedule> VacationSchedules => Set<VacationSchedule>();
public DbSet<OrganizationalPermission> OrganizationalPermissions => Set<OrganizationalPermission>();
```

**W metodzie OnModelCreating dodaj**:
```csharp
modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
modelBuilder.ApplyConfiguration(new VacationScheduleConfiguration());
modelBuilder.ApplyConfiguration(new OrganizationalPermissionConfiguration());
```

#### ✅ Kryteria akceptacji Task 1.6:
- ✅ Wszystkie 3 pliki konfiguracyjne utworzone
- ✅ Navigation properties skonfigurowane poprawnie
- ✅ Indeksy na: ParentDepartmentId, HeadOfDepartmentId, IsActive, Name, StartDate, EndDate, UserId, Status
- ✅ Cascade delete rules poprawne (Restrict dla departments, SetNull dla heads, Cascade dla users)
- ✅ JSONB column dla VisibleDepartmentIds (PostgreSQL)
- ✅ Kod kompiluje się bez błędów

---

### SPRINT 1 - Task 1.7: Migration - AddOrganizationalStructure ⭐ CRITICAL

**Czas**: 1 godzina
**Zależności**: Task 1.6

#### Krok 1: Wygeneruj migrację

```bash
cd backend/PortalForge.Infrastructure
dotnet ef migrations add AddOrganizationalStructure --project ../PortalForge.Infrastructure --startup-project ../PortalForge.Api
```

#### Krok 2: Sprawdź wygenerowaną migrację

Plik powinien zawierać:
- Utworzenie tabeli `Departments`
- Dodanie kolumny `DepartmentId` do tabeli `Users` (nullable!)
- Foreign keys z proper cascade behaviors
- Indeksy

#### Krok 3: Zmodyfikuj User entity

**Plik**: `backend/PortalForge.Domain/Entities/User.cs`

**Dodaj**:
```csharp
// NEW: Link to Department entity
public Guid? DepartmentId { get; set; }
public Department? DepartmentEntity { get; set; }

// KEEP: Old string field temporarily for migration
// TODO: Remove after data migration complete
public string Department { get; set; } = string.Empty;
```

#### Krok 4: Zaktualizuj UserConfiguration

**Plik**: `backend/PortalForge.Infrastructure/Persistence/Configurations/UserConfiguration.cs`

**Dodaj relację**:
```csharp
// Department relationship
builder.HasOne(u => u.DepartmentEntity)
    .WithMany(d => d.Employees)
    .HasForeignKey(u => u.DepartmentId)
    .OnDelete(DeleteBehavior.SetNull);

builder.HasIndex(u => u.DepartmentId);
```

#### Krok 5: Uruchom migrację

```bash
dotnet ef database update --project ../PortalForge.Infrastructure --startup-project ../PortalForge.Api
```

#### ✅ Kryteria akceptacji Task 1.7:
- ✅ Migracja uruchamia się bez błędów (`dotnet ef database update`)
- ✅ Można rollback bez błędów
- ✅ Istniejące dane User zachowane (kolumna Department string nadal istnieje)
- ✅ Można utworzyć department przez DbContext
- ✅ Można przypisać User do Department (DepartmentId)

---

### SPRINT 1 - Task 1.8: Migration - AddVacationScheduleSystem

**Czas**: 1.5 godziny
**Zależności**: Task 1.6

#### Krok 1: Wygeneruj migrację

```bash
cd backend/PortalForge.Infrastructure
dotnet ef migrations add AddVacationScheduleSystem --project ../PortalForge.Infrastructure --startup-project ../PortalForge.Api
```

#### Krok 2: Sprawdź wygenerowane tabele

Migracja powinna utworzyć:
- Tabela `VacationSchedules`
- Tabela `OrganizationalPermissions`

#### Krok 3: Rozszerz RequestApprovalStepTemplate

**Plik**: `backend/PortalForge.Domain/Entities/RequestApprovalStepTemplate.cs`

**Sprawdź czy jest już dodane pole** (agent mógł to już zrobić):
```csharp
/// <summary>
/// Specific department ID when ApproverType is SpecificDepartment.
/// The head of this department will be the approver.
/// </summary>
public Guid? SpecificDepartmentId { get; set; }
```

Jeśli NIE MA - dodaj to pole.

#### Krok 4: Rozszerz RequestTemplate

**Plik**: `backend/PortalForge.Domain/Entities/RequestTemplate.cs`

**Dodaj pole**:
```csharp
/// <summary>
/// Whether this request template requires substitute selection (e.g., for vacation requests).
/// If true, a UserSelect field will be auto-added to the template.
/// </summary>
public bool RequiresSubstituteSelection { get; set; } = false;
```

#### Krok 5: Zaktualizuj konfiguracje EF Core

**RequestApprovalStepTemplateConfiguration.cs** - dodaj:
```csharp
builder.Property(t => t.SpecificDepartmentId)
    .IsRequired(false);

builder.HasIndex(t => t.SpecificDepartmentId);
```

**RequestTemplateConfiguration.cs** - dodaj:
```csharp
builder.Property(t => t.RequiresSubstituteSelection)
    .IsRequired()
    .HasDefaultValue(false);
```

#### Krok 6: Uruchom migrację

```bash
dotnet ef database update --project ../PortalForge.Infrastructure --startup-project ../PortalForge.Api
```

#### ✅ Kryteria akceptacji Task 1.8:
- ✅ Wszystkie tabele utworzone
- ✅ Foreign keys działają (CASCADE, SET NULL, RESTRICT)
- ✅ Indeksy utworzone na: UserId, Status, StartDate, EndDate
- ✅ Kolumna JSONB działa (można insert/query JSON arrays)
- ✅ Pole SpecificDepartmentId dodane do RequestApprovalStepTemplate
- ✅ Pole RequiresSubstituteSelection dodane do RequestTemplate

---

## 📋 CO ZROBIĆ PO KAŻDYM ZADANIU

### 1. Odhacz checkbox w planie

**Plik**: `D:\Projects\PortalForge\.ai\implementation-plan-organizational-structure.md`

Zmień `[ ]` na `[x]`:
```markdown
#### [x] Task 1.6: EF Core Configurations
```

### 2. Zaktualizuj paski postępu

```markdown
**Sprint 1 (Backend Foundation)**: ██████░░ 6/8 tasks (75%)
**Progress**: 6/41 tasks complete (14.6%)
```

### 3. Commituj z proper message

```bash
git add .
git commit -m "feat: add EF Core configurations for organizational structure

Implement configurations for Department, VacationSchedule, and OrganizationalPermission:
- DepartmentConfiguration: self-referencing hierarchy with proper cascade rules
- VacationScheduleConfiguration: vacation tracking with indexes on date ranges
- OrganizationalPermissionConfiguration: JSONB column for department visibility

All navigation properties configured with appropriate delete behaviors.
Indexes added for frequently queried columns.

Task: Sprint 1, Task 1.6
Ref: .ai/implementation-plan-organizational-structure.md"
```

**Format commitów** (Conventional Commits):
- `feat:` - nowa funkcjonalność
- `fix:` - naprawa błędu
- `refactor:` - refaktoryzacja
- `test:` - testy
- `docs:` - dokumentacja

---

## ⚠️ WAŻNE ZASADY - MUSISZ PRZESTRZEGAĆ!

Z pliku `.claude/CLAUDE.md`:

### ❌ NIGDY NIE RÓB:
- ❌ **NIE dodawaj komentarzy "Generated with Claude Code"**
- ❌ **NIE dodawaj "Co-Authored-By: Claude"**
- ❌ Nie edytuj `appsettings.json` - używaj `appsettings.Development.json`
- ❌ Nie commituj sekretów, API keys, passwords
- ❌ Nie używaj `any` w TypeScript
- ❌ Nie twórz komend/zapytań bez walidatorów (FluentValidation)
- ❌ Nie dostęp do bazy bezpośrednio z kontrolerów - używaj MediatR

### ✅ ZAWSZE RÓB:
- ✅ Stosuj Clean Architecture (Domain → Application → Infrastructure → Api)
- ✅ Używaj CQRS z MediatR (Commands i Queries)
- ✅ FluentValidation dla wszystkich komend
- ✅ Repository Pattern + Unit of Work
- ✅ XML dokumentacja dla publicznych API
- ✅ **Odhaczaj checkboxy `[x]` po każdym zadaniu**
- ✅ **Aktualizuj paski postępu**
- ✅ Commituj zgodnie z Conventional Commits

---

## 📊 MONITORING POSTĘPU

Na końcu pliku `implementation-plan-organizational-structure.md` aktualizuj:

```markdown
### Overall Progress

Progress: ██████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 8/41 (19.5%)

**Completed Tasks**: 8/41
**In Progress**: 0
**Blocked**: 0
**Not Started**: 33
```

---

## 🎯 TWOJE NASTĘPNE KROKI

1. ✅ Przeczytaj `.claude/CLAUDE.md` i `.claude/backend.md`
2. ✅ Przeczytaj `.ai/implementation-plan-organizational-structure.md`
3. ✅ **Zacznij od Task 1.6** - Utwórz 3 pliki konfiguracyjne
4. ✅ Sprawdź kryteria akceptacji
5. ✅ **ODHACZ checkbox `[x]`**
6. ✅ **ZAKTUALIZUJ paski postępu**
7. ✅ Commituj
8. ✅ Przejdź do Task 1.7 (migracja)
9. ✅ Task 1.8 (druga migracja)
10. ✅ Dopiero POTEM przejdź do Sprint 2 (RequestRoutingService, VacationScheduleService)

---

## 🆘 Jeśli utkniesz

1. **Problem z EF Core?** → Sprawdź istniejące konfiguracje w `backend/PortalForge.Infrastructure/Persistence/Configurations/`
2. **Jak uruchomić migrację?** → Zobacz `.claude/CLAUDE.md` sekcja "Development Commands"
3. **Błędy kompilacji?** → Sprawdź czy wszystkie using statements są poprawne
4. **Migracja nie działa?** → `dotnet ef migrations remove` i spróbuj ponownie

---

## 🚀 ZACZYNAJ!

**START TUTAJ - Task 1.6:**
1. Utwórz `DepartmentConfiguration.cs`
2. Utwórz `VacationScheduleConfiguration.cs`
3. Utwórz `OrganizationalPermissionConfiguration.cs`
4. Zarejestruj w `ApplicationDbContext.cs`
5. Build projekt - sprawdź czy się kompiluje
6. **ODHACZ checkbox**
7. **ZAKTUALIZUJ progress**
8. Commituj
9. → Task 1.7 → Task 1.8 → Sprint 2

**Powodzenia! 💪**

---

**PAMIĘTAJ**: Bez ukończenia Tasks 1.6-1.8 nie będziesz mógł testować niczego w bazie danych. To FUNDAMENT całego systemu!
