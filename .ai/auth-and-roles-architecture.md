# PortalForge - Architektura Autoryzacji i Ról

## 📋 Obecny Stan Systemu

### 1. **Dual System: Supabase Auth + PortalForge Users**

```
┌─────────────────────────────────────────────────────────────┐
│                    SUPABASE AUTH                            │
│  - Przechowuje: email, password hash                        │
│  - Generuje: JWT tokens, refresh tokens                     │
│  - Zarządza: email verification, password reset             │
│  - Zwraca: user.id (UUID)                                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
                      user.id (UUID)
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  PORTALFORGE.USERS TABLE                    │
│  Id (Guid) → to samo co Supabase user.id                   │
│  Email                                                       │
│  FirstName, LastName                                         │
│  Role (Admin/Manager/HR/Marketing/Employee)                 │
│  Department, Position                                        │
│  SupervisorId                                               │
│  IsEmailVerified                                            │
│  CreatedAt, LastLoginAt                                     │
└─────────────────────────────────────────────────────────────┘
```

### 2. **Flow Rejestracji**
```
1. User wypełnia formularz rejestracji
2. POST /api/auth/register
   ↓
3. RegisterCommandHandler → SupabaseAuthService.RegisterAsync()
   ↓
4. Supabase tworzy auth user i zwraca UUID
   ↓
5. SupabaseAuthService tworzy rekord w PortalForge.Users z:
   - Id = Supabase UUID
   - Role = Employee (domyślnie!)
   - FirstName, LastName, Email, itp.
   ↓
6. Wysyłany jest email weryfikacyjny
```

### 3. **Flow Logowania**
```
1. User podaje email + password
2. POST /api/auth/login
   ↓
3. LoginCommandHandler → SupabaseAuthService.LoginAsync()
   ↓
4. Supabase weryfikuje credentials
   ↓
5. Supabase zwraca JWT token + user UUID
   ↓
6. Backend pobiera User z PortalForge.Users po UUID
   ↓
7. Do JWT dodawane są custom claims:
   - user_id: UUID
   - role: Admin/Manager/HR/Marketing/Employee
   - email: user@example.com
   ↓
8. Frontend dostaje: { user, accessToken, refreshToken }
```

### 4. **Flow Autoryzacji**
```
Frontend request z JWT token
   ↓
Backend middleware weryfikuje JWT
   ↓
Sprawdza claim "role" w tokenie
   ↓
[Authorize(Roles = "Admin,HR")] sprawdza czy user ma odpowiednią rolę
   ↓
Jeśli TAK → dostęp do endpoint
Jeśli NIE → 403 Forbidden
```

## 🔥 PROBLEM: Brak Systemu Zarządzania Rolami!

### Co jest źle:
1. ❌ **Nie ma endpoint do zmiany roli usera**
2. ❌ **Nie ma panelu admin do zarządzania userami**
3. ❌ **Wszyscy nowi userzy mają rolę "Employee"**
4. ❌ **Nie można promować usera na Admina przez API**
5. ❌ **Trzeba ręcznie edytować bazę aby zmienić role**

### Co się dzieje teraz:
```sql
-- Każdy nowy user ma:
INSERT INTO public."Users" (Id, Role, ...)
VALUES ('uuid-here', 0, ...); -- 0 = Employee enum value

-- Nie ma API aby zmienić na:
UPDATE public."Users" SET Role = 0 WHERE Id = 'uuid'; -- Admin
```

## ✅ ROZWIĄZANIE: System Zarządzania Rolami

### Faza 1: Backend API (PRIORYTET)

#### 1.1 Dodać UseCases dla zarządzania userami:

```
Application/UseCases/Users/
├── Commands/
│   ├── UpdateUserRole/
│   │   ├── UpdateUserRoleCommand.cs
│   │   ├── UpdateUserRoleCommandHandler.cs
│   │   └── UpdateUserRoleCommandValidator.cs
│   ├── UpdateUserProfile/
│   │   ├── UpdateUserProfileCommand.cs
│   │   └── UpdateUserProfileCommandHandler.cs
│   └── DeleteUser/
│       ├── DeleteUserCommand.cs
│       └── DeleteUserCommandHandler.cs
└── Queries/
    ├── GetAllUsers/
    │   ├── GetAllUsersQuery.cs
    │   └── GetAllUsersQueryHandler.cs
    └── GetUserById/
        ├── GetUserByIdQuery.cs
        └── GetUserByIdQueryHandler.cs
```

#### 1.2 Dodać UsersController:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PaginatedUsersResponse>> GetAll(
        [FromQuery] UserRole? role = null,
        [FromQuery] string? department = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        // Zwróć listę userów z filtrowaniem
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        // Pobierz szczegóły usera
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")] // TYLKO ADMIN MOŻE ZMIENIAĆ ROLE!
    public async Task<ActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request)
    {
        // Zmień rolę usera
        var command = new UpdateUserRoleCommand
        {
            UserId = id,
            NewRole = request.Role
        };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id}/profile")]
    [Authorize] // Każdy może edytować swój profil
    public async Task<ActionResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateUserProfileRequest request)
    {
        // Sprawdź czy user edytuje swój własny profil lub jest adminem
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (id.ToString() != currentUserId && currentUserRole != "Admin")
        {
            return Forbid();
        }

        // Zaktualizuj profil
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        // Usuń usera (soft delete lub hard delete)
    }
}
```

### Faza 2: Frontend Panel Administracyjny

#### 2.1 Struktura panelu:

```
pages/dashboard/admin/
├── users/
│   ├── index.vue          # Lista wszystkich userów
│   ├── [id].vue           # Szczegóły usera
│   └── [id]/edit.vue      # Edycja usera (profil + rola)
└── settings.vue           # Ustawienia systemu
```

#### 2.2 Funkcje panelu:

**Lista userów (`/dashboard/admin/users`):**
- Tabela z wszystkimi userami
- Filtry: rola, department, status email verification
- Wyszukiwanie po email/name
- Sortowanie
- Paginacja
- Actions: Edytuj, Zmień rolę, Usuń

**Edycja roli:**
- Modal z dropdown: Admin / Manager / HR / Marketing / Employee
- Zapisz → PUT /api/users/{id}/role
- Audit log: kto, kiedy, z jakiej roli na jaką zmienił

**Edycja profilu:**
- Formularz: FirstName, LastName, Department, Position, PhoneNumber
- Supervisor (dropdown z userami)
- Zapisz → PUT /api/users/{id}/profile

#### 2.3 Middleware dla admina:

```typescript
// middleware/admin.ts
export default defineNuxtRouteMiddleware((to, from) => {
  const authStore = useAuthStore()

  if (!authStore.isAuthenticated) {
    return navigateTo('/auth/login')
  }

  if (authStore.user?.role !== 'Admin') {
    return navigateTo('/dashboard')
  }
})

// Użycie:
definePageMeta({
  middleware: ['auth', 'admin']
})
```

## 🚀 Plan Implementacji

### Krok 1: Najpierw napraw logowanie (teraz!)
```bash
# Zarejestruj pierwszego admina ręcznie:
1. POST /api/auth/register z danymi
2. Zweryfikuj email
3. UPDATE public."Users" SET "Role" = 0 WHERE "Email" = 'admin@portalforge.com';
   -- 0 = Admin w enum
4. Teraz możesz się zalogować jako admin
```

### Krok 2: Zaimplementuj backend API dla zarządzania userami
- [ ] UpdateUserRoleCommand + Handler
- [ ] GetAllUsersQuery + Handler
- [ ] UsersController z endpoints
- [ ] Testy jednostkowe

### Krok 3: Zaimplementuj frontend panel admin
- [ ] Stronę listy userów
- [ ] Formularz zmiany roli
- [ ] Middleware admin
- [ ] E2E testy

### Krok 4: Dodaj audit log
- [ ] Tabela UserRoleChanges
- [ ] Historia zmian ról
- [ ] Wyświetlanie w panelu admin

## 📝 Najlepsze Praktyki

### 1. **Bezpieczeństwo**
- ✅ Tylko Admin może zmieniać role
- ✅ User może edytować tylko swój profil (chyba że Admin)
- ✅ Audit log wszystkich zmian ról
- ✅ Weryfikacja JWT przed każdym requestem
- ✅ Refresh token rotation

### 2. **Role Hierarchy**
```
Admin > HR > Manager > Marketing > Employee

Admin może:
- Wszystko

HR może:
- Zarządzać userami (dodawać, edytować profile)
- NIE może zmieniać ról (to tylko Admin)
- Zarządzać newsami HR

Manager może:
- Edytować swoich podwładnych
- Zatwierdzać urlopy

Marketing może:
- Zarządzać newsami Marketing
- Tworzyć wydarzenia

Employee może:
- Oglądać newsy
- Przeglądać organizację
- Edytować swój profil
```

### 3. **Supabase + PortalForge Sync**
- ✅ Zawsze synchro

nizuj User.Id z Supabase UUID
- ✅ Role przechowuj TYLKO w PortalForge.Users (nie w Supabase)
- ✅ JWT custom claims generuj z PortalForge.Users
- ✅ Email verification status sync z Supabase

## 🔍 Podsumowanie

**Obecny stan:**
- ✅ Supabase Auth działa (login/register)
- ✅ PortalForge.Users przechowuje role
- ✅ JWT authorization działa
- ❌ BRAK systemu zarządzania rolami
- ❌ BRAK panelu admin

**Co trzeba zrobić:**
1. Ręcznie utworzyć pierwszego admina w bazie
2. Zaimplementować UsersController z endpoints
3. Zaimplementować panel admin w frontend
4. Dodać middleware admin
5. Przetestować cały flow

**Priorytet:**
1. NAJPIERW: Napraw logowanie (zarejestruj admina)
2. POTEM: Zaimplementuj backend API
3. OSTATNIE: Zbuduj frontend panel
