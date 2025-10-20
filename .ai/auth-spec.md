# Specyfikacja Architektury Uwierzytelniania - PortalForge

## 1. Wprowadzenie

### 1.1 Cel dokumentu
Dokument definiuje architekturę systemu uwierzytelniania dla aplikacji PortalForge, bazującą na Supabase Auth z integracją w środowisku Nuxt 3 (frontend) i .NET 8.0 (backend).

### 1.2 Zakres
Specyfikacja obejmuje:
- Przepływy logowania, rejestracji i odzyskiwania hasła
- Zarządzanie sesjami i tokenami JWT
- Middleware autoryzacyjny
- Integrację z Supabase Auth
- Zabezpieczenia i najlepsze praktyki

## 2. Architektura wysokopoziomowa

### 2.1 Komponenty systemu

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Frontend  │─────▶│   Backend    │─────▶│  Supabase   │
│  (Nuxt 3)   │◀─────│  (.NET 8.0)  │◀─────│    Auth     │
└─────────────┘      └──────────────┘      └─────────────┘
      │                      │                      │
      │                      │                      │
      ▼                      ▼                      ▼
  Browser                 API                  PostgreSQL
  Cookies              Endpoints               auth.users
```

### 2.2 Warstwa odpowiedzialności

| Warstwa | Odpowiedzialność |
|---------|------------------|
| **Frontend (Nuxt 3)** | UI komponentów auth, walidacja formularzy, zarządzanie stanem sesji klienta |
| **Backend (.NET 8.0)** | Walidacja biznesowa, pośrednictwo z Supabase Auth, middleware autoryzacyjny |
| **Supabase Auth** | Przechowywanie użytkowników, hashowanie haseł, generowanie tokenów JWT |
| **PostgreSQL** | Persistencja danych użytkowników w schemacie auth.* |

## 3. Przepływy uwierzytelniania

### 3.1 Rejestracja użytkownika

**Przepływ:**
1. Użytkownik wypełnia formularz rejestracji (email, hasło, potwierdzenie hasła)
2. Frontend waliduje dane (frontend validation)
3. Żądanie POST `/api/auth/register` do backendu
4. Backend waliduje dane (FluentValidation)
5. Backend wywołuje Supabase Auth API `signUp()`
6. Supabase tworzy użytkownika w bazie i wysyła email weryfikacyjny
7. Backend zwraca sukces/błąd do frontendu
8. Użytkownik otrzymuje email z linkiem weryfikacyjnym
9. Po kliknięciu link aktywuje konto

**Endpointy Backend (.NET 8.0):**
```csharp
// POST /api/auth/register
public class RegisterCommand : IRequest<AuthResult>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
    }
}
```

**Endpointy Frontend (Nuxt 3):**
```typescript
// pages/auth/register.vue
async function handleRegister() {
  const { data, error } = await useFetch('/api/auth/register', {
    method: 'POST',
    body: {
      email: email.value,
      password: password.value,
      confirmPassword: confirmPassword.value
    }
  });

  if (!error.value) {
    navigateTo('/auth/verify-email');
  }
}
```

### 3.2 Logowanie użytkownika

**Przepływ:**
1. Użytkownik wypełnia formularz logowania (email, hasło)
2. Frontend waliduje dane
3. Żądanie POST `/api/auth/login` do backendu
4. Backend wywołuje Supabase Auth API `signInWithPassword()`
5. Supabase weryfikuje dane i generuje access_token + refresh_token
6. Backend ustawia ciasteczka HTTP-only z tokenami
7. Backend zwraca informacje o użytkowniku
8. Frontend przekierowuje do głównej strony aplikacji

**Endpointy Backend (.NET 8.0):**
```csharp
// POST /api/auth/login
public class LoginCommand : IRequest<AuthResult>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly ISupabaseClient _supabase;

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var response = await _supabase.Auth.SignInWithPassword(
            request.Email,
            request.Password
        );

        if (response.User == null)
            throw new UnauthorizedException("Invalid credentials");

        return new AuthResult
        {
            UserId = response.User.Id,
            Email = response.User.Email,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken
        };
    }
}
```

**Endpointy Frontend (Nuxt 3):**
```typescript
// pages/auth/login.vue
async function handleLogin() {
  const { data, error } = await useFetch('/api/auth/login', {
    method: 'POST',
    body: {
      email: email.value,
      password: password.value
    }
  });

  if (!error.value) {
    navigateTo('/');
  }
}
```

### 3.3 Wylogowanie użytkownika

**Przepływ:**
1. Użytkownik klika przycisk "Wyloguj"
2. Żądanie POST `/api/auth/logout` do backendu
3. Backend wywołuje Supabase Auth API `signOut()`
4. Backend usuwa ciasteczka sesyjne
5. Frontend przekierowuje do strony logowania

### 3.4 Odzyskiwanie hasła

**Przepływ:**
1. Użytkownik klika "Zapomniałeś hasła?" na stronie logowania
2. Użytkownik podaje email
3. Żądanie POST `/api/auth/reset-password` do backendu
4. Backend wywołuje Supabase Auth API `resetPasswordForEmail()`
5. Supabase wysyła email z linkiem resetującym (ważny 1h)
6. Użytkownik klika link i jest przekierowywany na stronę zmiany hasła
7. Użytkownik podaje nowe hasło
8. Backend wywołuje Supabase Auth API `updateUser()`
9. Wszystkie aktywne sesje są wylogowywane

## 4. Zarządzanie sesjami

### 4.1 Tokeny JWT

**Struktura tokenów:**
- **Access Token**: Krótkotrwały token (1h), przechowywany w localStorage
- **Refresh Token**: Długotrwały token (7 dni), przechowywany w localStorage

**Uwaga**: Tokeny przechowywane w localStorage (nie w cookies) dla uproszczenia MVP. W produkcji rozważyć migrację na HTTP-only cookies dla większego bezpieczeństwa.

**Payload Access Token:**
```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "role": "authenticated",
  "iat": 1234567890,
  "exp": 1234571490
}
```

### 4.2 Odświeżanie tokenów

**Przepływ automatyczny (Frontend):**
1. Plugin `token-refresh.ts` uruchamia się co 50 minut (tokeny ważne 1h)
2. Wywołuje `useAuth().refreshToken()`
3. Wysyła żądanie POST `/api/auth/refresh-token` z refresh_token z localStorage
4. Backend wywołuje Supabase Auth API `refreshSession()`
5. Supabase generuje nowy access_token + refresh_token
6. Frontend zapisuje nowe tokeny do localStorage
7. Proces powtarza się automatycznie

**Endpoint Backend (.NET 8.0):**
```csharp
// POST /api/auth/refresh-token
public class RefreshTokenCommand : IRequest<AuthResult>
{
    public string RefreshToken { get; set; }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly ISupabaseAuthService _authService;

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
            throw new UnauthorizedException("Token refresh failed");

        return result;
    }
}
```

**Frontend (Nuxt 3):**
```typescript
// composables/useAuth.ts
const refreshToken = async () => {
  const currentRefreshToken = authStore.refreshToken;

  const { data, error } = await useFetch('/api/auth/refresh-token', {
    method: 'POST',
    body: { refreshToken: currentRefreshToken }
  });

  if (!error.value && data.value) {
    // Update tokens in store (persists to localStorage)
    authStore.setTokens(data.value.accessToken, data.value.refreshToken);
  }
};

// plugins/token-refresh.ts - Auto refresh every 50 minutes
const REFRESH_INTERVAL = 50 * 60 * 1000;
setInterval(() => refreshToken(), REFRESH_INTERVAL);
```

## 5. Middleware autoryzacyjny

### 5.1 Backend Middleware (.NET 8.0)

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISupabaseClient _supabase;

    public async Task InvokeAsync(HttpContext context)
    {
        // Pobierz token z ciasteczka
        var accessToken = context.Request.Cookies["sb-access-token"];

        if (string.IsNullOrEmpty(accessToken))
        {
            await _next(context);
            return;
        }

        try
        {
            // Weryfikuj token
            var user = await _supabase.Auth.GetUser(accessToken);

            if (user != null)
            {
                // Dodaj użytkownika do context
                context.Items["User"] = user;
            }
        }
        catch (Exception ex)
        {
            // Token nieprawidłowy lub wygasły
            _logger.LogWarning(ex, "Token validation failed");
        }

        await _next(context);
    }
}
```

### 5.2 Frontend Middleware (Nuxt 3)

```typescript
// middleware/auth.ts - Podstawowa ochrona tras
export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore();

  if (!authStore.isAuthenticated && to.path !== '/auth/login') {
    return navigateTo('/auth/login');
  }
});

// middleware/verified.ts - Wymaga weryfikacji emaila
export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore();

  if (!authStore.isAuthenticated) {
    return navigateTo('/auth/login');
  }

  if (!authStore.user?.isEmailVerified && to.path !== '/auth/verify-email') {
    return navigateTo('/auth/verify-email?email=' + authStore.user?.email);
  }
});

// middleware/guest.ts - Blokuje zalogowanych użytkowników
export default defineNuxtRouteMiddleware(() => {
  const authStore = useAuthStore();

  if (authStore.isAuthenticated) {
    return navigateTo('/');
  }
});
```

## 6. Struktura projektu

### 6.1 Backend (.NET 8.0)

```
backend/
├── PortalForge.Api/
│   ├── Controllers/
│   │   └── AuthController.cs
│   ├── Middleware/
│   │   └── AuthenticationMiddleware.cs
│   └── Program.cs
├── PortalForge.Application/
│   ├── UseCases/
│   │   └── Auth/
│   │       ├── Commands/
│   │       │   ├── Login/
│   │       │   │   ├── LoginCommand.cs
│   │       │   │   ├── LoginCommandHandler.cs
│   │       │   │   └── Validation/
│   │       │   │       └── LoginCommandValidator.cs
│   │       │   ├── Register/
│   │       │   │   ├── RegisterCommand.cs
│   │       │   │   ├── RegisterCommandHandler.cs
│   │       │   │   └── Validation/
│   │       │   │       └── RegisterCommandValidator.cs
│   │       │   ├── RefreshToken/
│   │       │   │   ├── RefreshTokenCommand.cs
│   │       │   │   ├── RefreshTokenCommandHandler.cs
│   │       │   │   └── Validation/
│   │       │   │       └── RefreshTokenCommandValidator.cs
│   │       │   └── ResetPassword/
│   │       ├── Queries/
│   │       │   └── GetCurrentUser/
│   │       └── DTOs/
│   │           ├── AuthResponseDto.cs
│   │           ├── RefreshTokenRequestDto.cs
│   │           └── RefreshTokenResponseDto.cs
│   └── Common/
│       └── Interfaces/
│           └── ISupabaseAuthService.cs
└── PortalForge.Infrastructure/
    └── Auth/
        └── SupabaseClient.cs
```

### 6.2 Frontend (Nuxt 3)

```
frontend/
├── components/
│   └── auth/
│       ├── LoginForm.vue
│       ├── RegisterForm.vue
│       └── ResetPasswordForm.vue
├── composables/
│   └── useAuth.ts
├── layouts/
│   └── auth.vue
├── middleware/
│   ├── auth.ts
│   ├── guest.ts
│   └── verified.ts
├── pages/
│   ├── index.vue (dashboard)
│   └── auth/
│       ├── login.vue
│       ├── register.vue
│       ├── reset-password.vue
│       ├── verify-email.vue
│       └── callback.vue
├── plugins/
│   ├── auth-hydration.ts
│   └── token-refresh.ts
├── stores/
│   └── auth.ts
└── types/
    └── auth.ts
```

## 7. Bezpieczeństwo

### 7.1 Najlepsze praktyki

1. **Tokeny w localStorage**: Tokeny przechowywane w localStorage (MVP). W produkcji rozważyć HTTP-only cookies dla większego bezpieczeństwa
2. **HTTPS**: Wszystkie połączenia przez HTTPS
3. **CSRF Protection**: Token CSRF dla wszystkich mutujących operacji
4. **Rate Limiting**: Ograniczenie liczby prób logowania (5/minutę)
5. **Hashowanie haseł**: Supabase używa bcrypt
6. **Walidacja dwuetapowa**: Frontend + Backend
7. **Sesje**: Automatyczne odświeżanie tokenów co 50 minut
8. **Hydration**: Odtwarzanie stanu auth z localStorage przy starcie aplikacji

### 7.2 Zmienne środowiskowe

**Backend (.env):**
```env
SUPABASE_URL=https://xxxxx.supabase.co
SUPABASE_SERVICE_ROLE_KEY=xxxxx
SUPABASE_ANON_KEY=xxxxx
JWT_SECRET=xxxxx
CORS_ORIGINS=http://localhost:3000,https://portalforge.com
```

**Frontend (.env):**
```env
NUXT_PUBLIC_SUPABASE_URL=https://xxxxx.supabase.co
NUXT_PUBLIC_SUPABASE_KEY=xxxxx
NUXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

## 8. Obsługa błędów

### 8.1 Kody błędów

| Kod HTTP | Scenariusz | Komunikat |
|----------|------------|-----------|
| 400 | Nieprawidłowe dane wejściowe | "Wprowadzone dane są nieprawidłowe" |
| 401 | Nieprawidłowe dane logowania | "Email lub hasło są nieprawidłowe" |
| 403 | Brak uprawnień | "Nie masz uprawnień do tej operacji" |
| 409 | Email już istnieje | "Konto z tym emailem już istnieje" |
| 429 | Za dużo prób | "Zbyt wiele prób logowania. Spróbuj ponownie za 5 minut" |
| 500 | Błąd serwera | "Wystąpił błąd serwera. Spróbuj ponownie później" |

### 8.2 Obsługa błędów w UI

```typescript
// composables/useAuth.ts
export const useAuth = () => {
  const error = ref<string | null>(null);

  const handleAuthError = (err: any) => {
    if (err.statusCode === 401) {
      error.value = 'Email lub hasło są nieprawidłowe';
    } else if (err.statusCode === 409) {
      error.value = 'Konto z tym emailem już istnieje';
    } else if (err.statusCode === 429) {
      error.value = 'Zbyt wiele prób. Spróbuj ponownie za 5 minut';
    } else {
      error.value = 'Wystąpił nieoczekiwany błąd';
    }
  };

  return { error, handleAuthError };
};
```

## 9. Testowanie

### 9.1 Testy jednostkowe (Backend)

```csharp
[Fact]
public async Task Handle_ValidCredentials_ReturnsAuthResult()
{
    // Arrange
    var command = new LoginCommand
    {
        Email = "test@example.com",
        Password = "Password123!"
    };

    var mockSupabase = new Mock<ISupabaseClient>();
    mockSupabase.Setup(x => x.Auth.SignInWithPassword(
        It.IsAny<string>(),
        It.IsAny<string>()))
        .ReturnsAsync(new AuthResponse { User = new User() });

    var handler = new LoginCommandHandler(mockSupabase.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.UserId.Should().NotBeEmpty();
}
```

### 9.2 Testy E2E (Playwright)

```typescript
// tests/e2e/auth.spec.ts
test('should login with valid credentials', async ({ page }) => {
  await page.goto('/auth/login');

  await page.fill('input[name="email"]', 'test@example.com');
  await page.fill('input[name="password"]', 'Password123!');
  await page.click('button[type="submit"]');

  await expect(page).toHaveURL('/');
  await expect(page.getByText('Witaj, test@example.com')).toBeVisible();
});
```

## 10. Migracja i deployment

### 10.1 Konfiguracja Supabase

1. Utwórz projekt w Supabase Dashboard
2. Skopiuj Project URL i API Keys
3. Skonfiguruj Email Templates dla weryfikacji i resetowania hasła
4. Ustaw Redirect URLs w Auth Settings:
   - Development: `http://localhost:3000/auth/callback`
   - Production: `https://portalforge.com/auth/callback`

### 10.2 Deployment

**Backend:**
- Ustaw zmienne środowiskowe w środowisku produkcyjnym
- Skonfiguruj CORS dla domeny frontendu
- Włącz HTTPS
- Skonfiguruj rate limiting

**Frontend:**
- Zbuduj aplikację: `npm run build`
- Ustaw zmienne środowiskowe produkcyjne
- Deploy na VPS lub Cloudflare Pages

---

*Dokument utworzony: 2025-10-16*
*Ostatnia aktualizacja: 2025-10-20*
*Wersja: 1.1*
*Zmiany w wersji 1.1:*
- Dodano implementację refresh token z automatycznym odświeżaniem
- Zmiana przechowywania tokenów z cookies na localStorage (MVP)
- Dodano pluginy auth-hydration i token-refresh
- Zaktualizowano strukturę projektu
