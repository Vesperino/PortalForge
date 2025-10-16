# Implementacja Backendu Autoryzacji - PortalForge .NET 8.0

## Kontekst projektu

Jesteś doświadczonym architektem .NET odpowiedzialnym za implementację backendu autoryzacji dla systemu PortalForge. Projekt używa:
- .NET 8.0 z Clean Architecture
- CQRS z MediatR
- Entity Framework Core (Code First)
- PostgreSQL przez Supabase
- Supabase Auth dla uwierzytelniania
- MailKit dla wysyłki emaili

## Wymagane pliki kontekstowe

Przed rozpoczęciem przeanalizuj następujące dokumenty:
- `@.ai/prd.md` - Product Requirements Document (szczególnie US-001, US-002)
- `@.ai/auth-spec.md` - Specyfikacja architektury autoryzacji
- `@.ai/diagrams/auth.md` - Diagram przepływu autentykacji
- `@.ai/diagrams/journey.md` - Diagram podróży użytkownika
- `@.claude/CLAUDE.md` - Główne zasady projektu
- `@.claude/backend.md` - Standardy backend .NET 8.0
- `@.claude/testing.md` - Standardy testowania

## Cel zadania

Zaimplementuj pełny backend autoryzacji zgodnie z architekturą Clean Architecture + CQRS, integrując go z Supabase Auth i własnym systemem email.

---

## ETAP 0: Analiza mechanizmu walidacji z WatchdogAPI

### Zadanie 0.1: Zrozumienie architektury walidacji

Na podstawie projektu WatchdogAPI zaimplementuj identyczny mechanizm walidacji:

**IUnifiedValidatorService** - Automatyczny mechanizm znajdowania i wykonywania walidatorów:
```csharp
// Application/Common/Interfaces/IUnifiedValidatorService.cs
public interface IUnifiedValidatorService
{
    Task ValidateAsync<T>(T instance) where T : class;
}
```

**UnifiedValidatorService Implementation:**
```csharp
// Infrastructure/Validation/UnifiedValidatorService.cs
public class UnifiedValidatorService : IUnifiedValidatorService
{
    private readonly IServiceProvider _serviceProvider;

    public UnifiedValidatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ValidateAsync<T>(T instance) where T : class
    {
        var validatorType = typeof(IValidator<T>);
        var validator = _serviceProvider.GetService(validatorType) as IValidator<T>;

        if (validator == null)
        {
            // Brak walidatora - kontynuuj
            return;
        }

        var validationResult = await validator.ValidateAsync(instance);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            throw new ValidationException(
                "Validation failed",
                validationResult.Errors);
        }
    }
}
```

**ValidatorExtension - Automatyczna rejestracja:**
```csharp
// Application/Extensions/ValidatorExtension.cs
public static class ValidatorExtension
{
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(t => t.Interface.IsGenericType &&
                       t.Interface.GetGenericTypeDefinition() == typeof(IValidator<>))
            .ToList();

        foreach (var validator in validatorTypes)
        {
            services.AddTransient(validator.Interface, validator.Type);
        }

        return services;
    }
}
```

**Użycie w Handler (jak w WatchdogAPI):**
```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IMapper _mapper;

    public async Task<AuthResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Walidacja przez UnifiedValidatorService
        await _validatorService.ValidateAsync(request);

        // 2. Logika biznesowa
        var authResponse = await _authService.SignInWithPasswordAsync(
            request.Email,
            request.Password);

        return _mapper.Map<AuthResponseDto>(authResponse);
    }
}
```

**Output:** Zrozumienie i przygotowanie do implementacji mechanizmu walidacji.

---

## ETAP 1: Planowanie pełnej struktury projektu

### Zadanie 1.1: Struktura modułu Auth w Application Layer

```
PortalForge.Application/
├── Auth/
│   ├── Commands/
│   │   ├── Login/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── LoginCommandHandler.cs
│   │   │   └── Validation/
│   │   │       └── LoginCommandValidator.cs
│   │   ├── Register/
│   │   │   ├── RegisterCommand.cs
│   │   │   ├── RegisterCommandHandler.cs
│   │   │   └── Validation/
│   │   │       └── RegisterCommandValidator.cs
│   │   ├── Logout/
│   │   │   ├── LogoutCommand.cs
│   │   │   └── LogoutCommandHandler.cs
│   │   ├── ResetPassword/
│   │   │   ├── ResetPasswordCommand.cs
│   │   │   ├── ResetPasswordCommandHandler.cs
│   │   │   └── Validation/
│   │   │       └── ResetPasswordCommandValidator.cs
│   │   └── VerifyEmail/
│   │       ├── VerifyEmailCommand.cs
│   │       └── VerifyEmailCommandHandler.cs
│   ├── Queries/
│   │   └── GetCurrentUser/
│   │       ├── GetCurrentUserQuery.cs
│   │       └── GetCurrentUserQueryHandler.cs
│   └── DTOs/
│       ├── AuthResponseDto.cs
│       ├── LoginRequestDto.cs
│       ├── RegisterRequestDto.cs
│       ├── ResetPasswordRequestDto.cs
│       └── UserDto.cs
├── Common/
│   ├── Interfaces/
│   │   ├── ISupabaseAuthService.cs
│   │   ├── ICurrentUserService.cs
│   │   ├── IEmailService.cs
│   │   └── IUnifiedValidatorService.cs
│   ├── Mappings/
│   │   └── AuthMappingProfile.cs
│   └── Exceptions/
│       ├── ValidationException.cs
│       ├── UnauthorizedException.cs
│       └── AuthenticationException.cs
└── Extensions/
    └── ValidatorExtension.cs
```

### Zadanie 1.2: Struktura Infrastructure Layer

```
PortalForge.Infrastructure/
├── Auth/
│   ├── SupabaseAuthService.cs
│   └── CurrentUserService.cs
├── Email/
│   ├── EmailService.cs
│   ├── Models/
│   │   └── EmailMessage.cs
│   └── Templates/
│       ├── VerificationEmail.html
│       ├── PasswordResetEmail.html
│       └── PasswordChangedEmail.html
├── Validation/
│   └── UnifiedValidatorService.cs
└── Persistence/
    ├── ApplicationDbContext.cs
    └── Configurations/
        └── UserConfiguration.cs
```

### Zadanie 1.3: Struktura API Layer

```
PortalForge.Api/
├── Controllers/
│   └── AuthController.cs
├── Middleware/
│   └── AuthenticationMiddleware.cs
└── appsettings.Development.json (z konfiguracją SMTP)
```

### Zadanie 1.4: Struktura projektu testowego (NOWE)

Stwórz nowy projekt testowy:

```
PortalForge.Tests/
├── PortalForge.Tests.csproj
├── Unit/
│   ├── Application/
│   │   └── Auth/
│   │       ├── Commands/
│   │       │   ├── LoginCommandHandlerTests.cs
│   │       │   ├── RegisterCommandHandlerTests.cs
│   │       │   ├── LogoutCommandHandlerTests.cs
│   │       │   └── ResetPasswordCommandHandlerTests.cs
│   │       └── Validators/
│   │           ├── LoginCommandValidatorTests.cs
│   │           ├── RegisterCommandValidatorTests.cs
│   │           └── ResetPasswordCommandValidatorTests.cs
│   ├── Infrastructure/
│   │   ├── Auth/
│   │   │   └── SupabaseAuthServiceTests.cs
│   │   ├── Email/
│   │   │   └── EmailServiceTests.cs
│   │   └── Validation/
│   │       └── UnifiedValidatorServiceTests.cs
│   └── Helpers/
│       ├── MockHelper.cs
│       └── TestDataBuilder.cs
└── Integration/
    └── Auth/
        ├── AuthControllerTests.cs
        └── AuthFlowTests.cs
```

**PortalForge.Tests.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xUnit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\backend\PortalForge.Application\PortalForge.Application.csproj" />
    <ProjectReference Include="..\backend\PortalForge.Infrastructure\PortalForge.Infrastructure.csproj" />
    <ProjectReference Include="..\backend\PortalForge.Api\PortalForge.Api.csproj" />
  </ItemGroup>
</Project>
```

### Zadanie 1.5: Zapisz decision o strukturze

Stwórz `.ai/decisions/002-auth-backend-structure.md`:
```markdown
# ADR 002: Struktura Backendu Autoryzacji

## Status
Zaakceptowane - 2025-01-16

## Kontekst
Implementacja backendu autoryzacji z integracją Supabase Auth i własnym systemem email.

## Decyzja

### Architektura walidacji
- Użycie IUnifiedValidatorService (wzorowane na WatchdogAPI)
- Automatyczna rejestracja validatorów przez reflection
- Validators w folderach `Validation/` obok Handlers
- FluentValidation z async validation

### Email Service
- Lokalizacja: Infrastructure/Email/
- Biblioteka: MailKit (najbardziej dojrzała)
- Szablony HTML jako embedded resources
- 3 szablony: Verification, PasswordReset, PasswordChanged

### Supabase Integration
- Supabase Auth dla tokenów JWT
- Własny SMTP dla wysyłki emaili (pełna kontrola)
- HTTP-only cookies dla przechowywania tokenów

### Struktura projektu testowego
- Osobny projekt PortalForge.Tests
- Podział na Unit i Integration
- xUnit + FluentAssertions + Moq
- TestDataBuilder pattern dla złożonych obiektów

## Konsekwencje

### Pozytywne
- Czysty podział odpowiedzialności
- Automatyczna walidacja bez boilerplate
- Łatwe testowanie (DI-friendly)
- Pełna kontrola nad emailami
- Bezpieczne przechowywanie tokenów
- Izolowane testy jednostkowe

### Negatywne
- Dodatkowa złożoność EmailService
- Konieczność zarządzania SMTP credentials
- Więcej projektów w solution
```

**Output:** Kompletna struktura projektu backend + tests.

---

## ETAP 2: Implementacja IUnifiedValidatorService

### Zadanie 2.1: Interfejs i implementacja

Stwórz interfejs w `Application/Common/Interfaces/IUnifiedValidatorService.cs`:
```csharp
namespace PortalForge.Application.Common.Interfaces;

public interface IUnifiedValidatorService
{
    Task ValidateAsync<T>(T instance) where T : class;
}
```

Implementacja w `Infrastructure/Validation/UnifiedValidatorService.cs`:
```csharp
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.Validation;

public class UnifiedValidatorService : IUnifiedValidatorService
{
    private readonly IServiceProvider _serviceProvider;

    public UnifiedValidatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ValidateAsync<T>(T instance) where T : class
    {
        var validatorType = typeof(IValidator<T>);
        var validator = _serviceProvider.GetService(validatorType) as IValidator<T>;

        if (validator == null)
        {
            // No validator registered for this type - continue without validation
            return;
        }

        var validationResult = await validator.ValidateAsync(instance);

        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }
    }
}
```

### Zadanie 2.2: ValidatorExtension

Stwórz `Application/Extensions/ValidatorExtension.cs`:
```csharp
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PortalForge.Application.Extensions;

public static class ValidatorExtension
{
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(t => t.Interface.IsGenericType &&
                       t.Interface.GetGenericTypeDefinition() == typeof(IValidator<>))
            .ToList();

        foreach (var validator in validatorTypes)
        {
            services.AddTransient(validator.Interface, validator.Type);
        }

        return services;
    }
}
```

### Zadanie 2.3: Rejestracja w DI

W `Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<IUnifiedValidatorService, UnifiedValidatorService>();
```

W `Application/DependencyInjection.cs`:
```csharp
services.AddValidators(Assembly.GetExecutingAssembly());
```

**Output:** Działający mechanizm automatycznej walidacji.

---

## ETAP 3: Implementacja EmailService

### Zadanie 3.1: Konfiguracja SMTP

Dodaj do `appsettings.Development.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.zenbox.pl",
    "SmtpPort": 587,
    "UseTLS": true,
    "Username": "portalforge@krablab.pl",
    "Password": "gst48fz@$Bvt",
    "FromEmail": "portalforge@krablab.pl",
    "FromName": "PortalForge - Portal Wewnętrzny"
  }
}
```

**UWAGA BEZPIECZEŃSTWA:** W produkcji użyj User Secrets lub Environment Variables!

### Zadanie 3.2: EmailSettings Model

Stwórz `Infrastructure/Email/Models/EmailSettings.cs`:
```csharp
namespace PortalForge.Infrastructure.Email.Models;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseTLS { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
```

### Zadanie 3.3: EmailMessage Model

Stwórz `Infrastructure/Email/Models/EmailMessage.cs`:
```csharp
namespace PortalForge.Infrastructure.Email.Models;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}
```

### Zadanie 3.4: IEmailService Interface

Stwórz `Application/Common/Interfaces/IEmailService.cs`:
```csharp
namespace PortalForge.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendPasswordChangedEmailAsync(string toEmail);
}
```

### Zadanie 3.5: Szablony HTML Email

**1. VerificationEmail.html:**
```html
<!DOCTYPE html>
<html lang="pl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Weryfikacja konta</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        .container {
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 30px;
        }
        .header {
            text-align: center;
            padding-bottom: 20px;
            border-bottom: 2px solid #2563eb;
        }
        .header h1 {
            color: #2563eb;
            margin: 0;
        }
        .content {
            padding: 20px 0;
        }
        .button {
            display: inline-block;
            padding: 12px 30px;
            background-color: #2563eb;
            color: white !important;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }
        .footer {
            text-align: center;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            font-size: 12px;
            color: #666;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>PortalForge</h1>
            <p>Portal Wewnętrzny Organizacji</p>
        </div>

        <div class="content">
            <h2>Witaj w PortalForge!</h2>
            <p>Dziękujemy za rejestrację. Aby aktywować swoje konto, kliknij w poniższy przycisk:</p>

            <div style="text-align: center;">
                <a href="{{VERIFICATION_LINK}}" class="button">Aktywuj konto</a>
            </div>

            <p>Lub skopiuj poniższy link do przeglądarki:</p>
            <p style="background-color: #fff; padding: 10px; border: 1px solid #ddd; word-break: break-all;">
                {{VERIFICATION_LINK}}
            </p>

            <p><strong>Link jest ważny przez 1 godzinę.</strong></p>

            <p>Jeśli nie zakładałeś konta w PortalForge, zignoruj tę wiadomość.</p>
        </div>

        <div class="footer">
            <p>&copy; 2025 PortalForge. Wszelkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>
```

**2. PasswordResetEmail.html:**
```html
<!DOCTYPE html>
<html lang="pl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Reset hasła</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        .container {
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 30px;
        }
        .header {
            text-align: center;
            padding-bottom: 20px;
            border-bottom: 2px solid #2563eb;
        }
        .header h1 {
            color: #2563eb;
            margin: 0;
        }
        .content {
            padding: 20px 0;
        }
        .button {
            display: inline-block;
            padding: 12px 30px;
            background-color: #2563eb;
            color: white !important;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }
        .warning {
            background-color: #fff3cd;
            border: 1px solid #ffc107;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }
        .footer {
            text-align: center;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            font-size: 12px;
            color: #666;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>PortalForge</h1>
            <p>Portal Wewnętrzny Organizacji</p>
        </div>

        <div class="content">
            <h2>Reset hasła</h2>
            <p>Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta.</p>

            <div style="text-align: center;">
                <a href="{{RESET_LINK}}" class="button">Zresetuj hasło</a>
            </div>

            <p>Lub skopiuj poniższy link do przeglądarki:</p>
            <p style="background-color: #fff; padding: 10px; border: 1px solid #ddd; word-break: break-all;">
                {{RESET_LINK}}
            </p>

            <div class="warning">
                <strong>⚠️ Ważne informacje:</strong>
                <ul>
                    <li>Link jest ważny przez 1 godzinę</li>
                    <li>Po zmianie hasła wszystkie aktywne sesje zostaną wylogowane</li>
                </ul>
            </div>

            <p>Jeśli nie prosiłeś o reset hasła, zignoruj tę wiadomość. Twoje konto jest bezpieczne.</p>
        </div>

        <div class="footer">
            <p>&copy; 2025 PortalForge. Wszelkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>
```

**3. PasswordChangedEmail.html:**
```html
<!DOCTYPE html>
<html lang="pl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hasło zostało zmienione</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        .container {
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 30px;
        }
        .header {
            text-align: center;
            padding-bottom: 20px;
            border-bottom: 2px solid #10b981;
        }
        .header h1 {
            color: #10b981;
            margin: 0;
        }
        .content {
            padding: 20px 0;
        }
        .success {
            background-color: #d1fae5;
            border: 1px solid #10b981;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
            text-align: center;
        }
        .info {
            background-color: #e0f2fe;
            border: 1px solid #0284c7;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }
        .footer {
            text-align: center;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            font-size: 12px;
            color: #666;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>PortalForge</h1>
            <p>Portal Wewnętrzny Organizacji</p>
        </div>

        <div class="content">
            <div class="success">
                <h2 style="color: #10b981; margin: 0;">✓ Hasło zostało zmienione</h2>
            </div>

            <p>Twoje hasło zostało pomyślnie zmienione.</p>

            <div class="info">
                <strong>ℹ️ Co się stało:</strong>
                <ul>
                    <li>Hasło do Twojego konta zostało zaktualizowane</li>
                    <li>Wszystkie aktywne sesje zostały wylogowane</li>
                    <li>Możesz teraz zalogować się używając nowego hasła</li>
                </ul>
            </div>

            <p><strong>Jeśli to nie Ty zmieniłeś hasło, natychmiast skontaktuj się z działem IT!</strong></p>

            <p>Data zmiany: {{DATE_TIME}}</p>
        </div>

        <div class="footer">
            <p>&copy; 2025 PortalForge. Wszelkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>
```

### Zadanie 3.6: Implementacja EmailService

Stwórz `Infrastructure/Email/EmailService.cs`:
```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Email.Models;
using System.Reflection;

namespace PortalForge.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        var template = GetEmailTemplate("VerificationEmail.html");
        var body = template.Replace("{{VERIFICATION_LINK}}", verificationLink);

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Aktywuj swoje konto w PortalForge",
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Verification email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var template = GetEmailTemplate("PasswordResetEmail.html");
        var body = template.Replace("{{RESET_LINK}}", resetLink);

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Resetowanie hasła w PortalForge",
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendPasswordChangedEmailAsync(string toEmail)
    {
        var template = GetEmailTemplate("PasswordChangedEmail.html");
        var body = template.Replace("{{DATE_TIME}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Hasło zostało zmienione - PortalForge",
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Password changed notification sent to {Email}", toEmail);
    }

    private async Task SendEmailAsync(EmailMessage emailMessage)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(emailMessage.To));
        mimeMessage.Subject = emailMessage.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = emailMessage.IsHtml ? emailMessage.Body : null,
            TextBody = !emailMessage.IsHtml ? emailMessage.Body : null
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                _emailSettings.UseTLS ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(mimeMessage);

            _logger.LogInformation("Email sent successfully to {To}", emailMessage.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", emailMessage.To);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private string GetEmailTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"PortalForge.Infrastructure.Email.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Email template not found: {templateName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
```

### Zadanie 3.7: Konfiguracja embedded resources

W `PortalForge.Infrastructure.csproj` dodaj:
```xml
<ItemGroup>
  <EmbeddedResource Include="Email\Templates\*.html" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="MailKit" Version="4.3.0" />
</ItemGroup>
```

### Zadanie 3.8: Rejestracja w DI

W `Infrastructure/DependencyInjection.cs`:
```csharp
services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
services.AddScoped<IEmailService, EmailService>();
```

**Output:** Działający EmailService z 3 szablonami HTML.

---

## ETAP 4: Implementacja schematów Entity Framework

### Zadanie 4.1: Encja User

Stwórz `Domain/Entities/User.cs`:
```csharp
namespace PortalForge.Domain.Entities;

public class User
{
    public Guid Id { get; set; }  // Guid z Supabase Auth
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; } = false;
}

public enum UserRole
{
    Admin,
    Manager,
    HR,
    Marketing,
    Employee
}
```

### Zadanie 4.2: Konfiguracja EF Core

Stwórz `Infrastructure/Persistence/Configurations/UserConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "public");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
```

### Zadanie 4.3: Aktualizuj ApplicationDbContext

Dodaj w `Infrastructure/Persistence/ApplicationDbContext.cs`:
```csharp
public DbSet<User> Users { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    base.OnModelCreating(modelBuilder);
}
```

### Zadanie 4.4: Stwórz migrację

```bash
cd backend/PortalForge.Infrastructure
dotnet ef migrations add AddUserEntity --startup-project ../PortalForge.Api
```

### Zadanie 4.5: Zastosuj migrację

```bash
dotnet ef database update --startup-project ../PortalForge.Api
```

**Output:** Encja User w bazie Supabase PostgreSQL.

---

## ETAP 5: Integracja z Supabase Auth

### Zadanie 5.1: ISupabaseAuthService Interface

Stwórz `Application/Common/Interfaces/ISupabaseAuthService.cs`:
```csharp
using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface ISupabaseAuthService
{
    Task<AuthResult> SignInWithPasswordAsync(string email, string password);
    Task<AuthResult> SignUpAsync(string email, string password, string confirmationUrl);
    Task SignOutAsync(string accessToken);
    Task<User?> GetUserAsync(string accessToken);
    Task<AuthResult> RefreshSessionAsync(string refreshToken);
    Task ResetPasswordForEmailAsync(string email, string resetUrl);
    Task UpdatePasswordAsync(string accessToken, string newPassword);
}

public class AuthResult
{
    public User User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

### Zadanie 5.2: Implementacja SupabaseAuthService

**UWAGA:** Sprawdź czy masz dostęp do MCP Supabase lub użyj Supabase C# SDK.

Stwórz `Infrastructure/Auth/SupabaseAuthService.cs`:
```csharp
using Microsoft.Extensions.Configuration;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using Supabase;
using Supabase.Gotrue;

namespace PortalForge.Infrastructure.Auth;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        IConfiguration configuration,
        ILogger<SupabaseAuthService> logger)
    {
        var url = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL not configured");
        var key = configuration["Supabase:AnonKey"] ?? throw new InvalidOperationException("Supabase Anon Key not configured");

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };

        _supabaseClient = new Client(url, key, options);
        _logger = logger;
    }

    public async Task<AuthResult> SignInWithPasswordAsync(string email, string password)
    {
        try
        {
            var session = await _supabaseClient.Auth.SignIn(email, password);

            if (session?.User == null)
                throw new UnauthorizedException("Invalid credentials");

            return new AuthResult
            {
                User = MapToUser(session.User),
                AccessToken = session.AccessToken ?? string.Empty,
                RefreshToken = session.RefreshToken ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign in user {Email}", email);
            throw new UnauthorizedException("Invalid email or password");
        }
    }

    public async Task<AuthResult> SignUpAsync(string email, string password, string confirmationUrl)
    {
        try
        {
            var session = await _supabaseClient.Auth.SignUp(email, password, new SignUpOptions
            {
                EmailRedirectTo = confirmationUrl
            });

            if (session?.User == null)
                throw new Exception("Failed to create user");

            return new AuthResult
            {
                User = MapToUser(session.User),
                AccessToken = session.AccessToken ?? string.Empty,
                RefreshToken = session.RefreshToken ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign up user {Email}", email);
            throw;
        }
    }

    public async Task SignOutAsync(string accessToken)
    {
        try
        {
            await _supabaseClient.Auth.SignOut(accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign out user");
            throw;
        }
    }

    public async Task<User?> GetUserAsync(string accessToken)
    {
        try
        {
            var user = await _supabaseClient.Auth.GetUser(accessToken);
            return user != null ? MapToUser(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user from token");
            return null;
        }
    }

    public async Task<AuthResult> RefreshSessionAsync(string refreshToken)
    {
        try
        {
            var session = await _supabaseClient.Auth.RefreshToken(refreshToken);

            if (session?.User == null)
                throw new UnauthorizedException("Invalid refresh token");

            return new AuthResult
            {
                User = MapToUser(session.User),
                AccessToken = session.AccessToken ?? string.Empty,
                RefreshToken = session.RefreshToken ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh session");
            throw new UnauthorizedException("Failed to refresh session");
        }
    }

    public async Task ResetPasswordForEmailAsync(string email, string resetUrl)
    {
        try
        {
            await _supabaseClient.Auth.ResetPasswordForEmail(email, new ResetPasswordOptions
            {
                RedirectTo = resetUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw;
        }
    }

    public async Task UpdatePasswordAsync(string accessToken, string newPassword)
    {
        try
        {
            await _supabaseClient.Auth.UpdateUser(accessToken, new UserAttributes
            {
                Password = newPassword
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update password");
            throw;
        }
    }

    private User MapToUser(Supabase.Gotrue.User supabaseUser)
    {
        return new User
        {
            Id = Guid.Parse(supabaseUser.Id),
            Email = supabaseUser.Email ?? string.Empty,
            CreatedAt = supabaseUser.CreatedAt,
            IsEmailVerified = supabaseUser.EmailConfirmedAt.HasValue,
            Role = UserRole.Employee // Default, can be updated later
        };
    }
}
```

### Zadanie 5.3: Konfiguracja Supabase

Dodaj do `appsettings.Development.json`:
```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "your-anon-key"
  }
}
```

### Zadanie 5.4: Instalacja pakietów NuGet

```bash
dotnet add package supabase-csharp
```

### Zadanie 5.5: Rejestracja w DI

W `Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();
```

**Output:** Pełna integracja z Supabase Auth.

---

## ETAP 6: Implementacja CQRS Commands

### Zadanie 6.1: LoginCommand

**LoginCommand.cs:**
```csharp
using MediatR;
using PortalForge.Application.Auth.DTOs;

namespace PortalForge.Application.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

**LoginCommandHandler.cs:**
```csharp
using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Auth.DTOs;
using AutoMapper;

namespace PortalForge.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ISupabaseAuthService authService,
        IUnifiedValidatorService validatorService,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _validatorService = validatorService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Walidacja
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        // 2. Logowanie przez Supabase Auth
        var authResult = await _authService.SignInWithPasswordAsync(
            request.Email,
            request.Password);

        _logger.LogInformation("User logged in successfully: {UserId}", authResult.User.Id);

        // 3. Mapowanie do DTO
        return new AuthResponseDto
        {
            User = _mapper.Map<UserDto>(authResult.User),
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken
        };
    }
}
```

**LoginCommandValidator.cs (w folderze Validation/):**
```csharp
using FluentValidation;

namespace PortalForge.Application.Auth.Commands.Login.Validation;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format email")
            .MaximumLength(255).WithMessage("Email nie może przekraczać 255 znaków");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane")
            .MinimumLength(8).WithMessage("Hasło musi mieć minimum 8 znaków");
    }
}
```

### Zadanie 6.2: RegisterCommand

**RegisterCommand.cs:**
```csharp
using MediatR;
using PortalForge.Application.Auth.DTOs;

namespace PortalForge.Application.Auth.Commands.Register;

public class RegisterCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

**RegisterCommandHandler.cs:**
```csharp
using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Auth.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace PortalForge.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        ISupabaseAuthService authService,
        IEmailService emailService,
        IUnifiedValidatorService validatorService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RegisterCommandHandler> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _validatorService = validatorService;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Walidacja
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Registration attempt for: {Email}", request.Email);

        // 2. Przygotuj URL weryfikacyjny
        var httpContext = _httpContextAccessor.HttpContext;
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var verificationUrl = $"{baseUrl}/auth/verify";

        // 3. Rejestracja w Supabase Auth
        var authResult = await _authService.SignUpAsync(
            request.Email,
            request.Password,
            verificationUrl);

        _logger.LogInformation("User registered successfully: {UserId}", authResult.User.Id);

        // 4. Wysłanie emaila weryfikacyjnego
        // Supabase wygeneruje token weryfikacyjny i przekieruje na nasz URL
        // My dodatkowo wysyłamy własny email z naszym SMTP
        var verificationLink = $"{verificationUrl}?token={authResult.AccessToken}";
        await _emailService.SendVerificationEmailAsync(request.Email, verificationLink);

        // 5. Mapowanie do DTO
        return new AuthResponseDto
        {
            User = _mapper.Map<UserDto>(authResult.User),
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken
        };
    }
}
```

**RegisterCommandValidator.cs:**
```csharp
using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.Auth.Commands.Register.Validation;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format email")
            .MaximumLength(255).WithMessage("Email nie może przekraczać 255 znaków")
            .MustAsync(BeUniqueEmail).WithMessage("Konto z tym adresem email już istnieje");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane")
            .MinimumLength(8).WithMessage("Hasło musi mieć minimum 8 znaków")
            .Matches(@"[A-Z]").WithMessage("Hasło musi zawierać przynajmniej jedną wielką literę")
            .Matches(@"[a-z]").WithMessage("Hasło musi zawierać przynajmniej jedną małą literę")
            .Matches(@"[0-9]").WithMessage("Hasło musi zawierać przynajmniej jedną cyfrę")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Hasło musi zawierać przynajmniej jeden znak specjalny");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Hasła muszą być identyczne");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        return existingUser == null;
    }
}
```

### Zadanie 6.3: LogoutCommand

**LogoutCommand.cs:**
```csharp
using MediatR;

namespace PortalForge.Application.Auth.Commands.Logout;

public class LogoutCommand : IRequest<Unit>
{
    // Token będzie pobierany z CurrentUserService
}
```

**LogoutCommandHandler.cs:**
```csharp
using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly ISupabaseAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ISupabaseAuthService authService,
        ICurrentUserService currentUserService,
        ILogger<LogoutCommandHandler> logger)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var accessToken = _currentUserService.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Logout attempt without access token");
            return Unit.Value;
        }

        _logger.LogInformation("User logout: {UserId}", _currentUserService.UserId);

        await _authService.SignOutAsync(accessToken);

        return Unit.Value;
    }
}
```

### Zadanie 6.4: ResetPasswordCommand

**ResetPasswordCommand.cs:**
```csharp
using MediatR;

namespace PortalForge.Application.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<Unit>
{
    public string Email { get; set; } = string.Empty;
}
```

**ResetPasswordCommandHandler.cs:**
```csharp
using MediatR;
using PortalForge.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace PortalForge.Application.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        ISupabaseAuthService authService,
        IEmailService emailService,
        IUnifiedValidatorService validatorService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _validatorService = validatorService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Walidacja
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Password reset requested for: {Email}", request.Email);

        // 2. Przygotuj URL resetu
        var httpContext = _httpContextAccessor.HttpContext;
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var resetUrl = $"{baseUrl}/auth/reset-password";

        // 3. Wywołaj Supabase Auth
        await _authService.ResetPasswordForEmailAsync(request.Email, resetUrl);

        // 4. Wysłanie własnego emaila
        // Token będzie w URL callback od Supabase
        await _emailService.SendPasswordResetEmailAsync(request.Email, resetUrl);

        return Unit.Value;
    }
}
```

**ResetPasswordCommandValidator.cs:**
```csharp
using FluentValidation;

namespace PortalForge.Application.Auth.Commands.ResetPassword.Validation;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format email");
    }
}
```

**Output:** Wszystkie Commands z Handlers i Validators w odpowiednich folderach.

---

## ETAP 7: Implementacja CQRS Queries

### Zadanie 7.1: GetCurrentUserQuery

**GetCurrentUserQuery.cs:**
```csharp
using MediatR;
using PortalForge.Application.Auth.DTOs;

namespace PortalForge.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserDto>
{
    // Token będzie pobierany z CurrentUserService
}
```

**GetCurrentUserQueryHandler.cs:**
```csharp
using MediatR;
using AutoMapper;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Auth.DTOs;

namespace PortalForge.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ISupabaseAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(
        ISupabaseAuthService authService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var accessToken = _currentUserService.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
            throw new UnauthorizedException("No active session");

        var user = await _authService.GetUserAsync(accessToken);

        if (user == null)
            throw new UnauthorizedException("Invalid or expired token");

        return _mapper.Map<UserDto>(user);
    }
}
```

**Output:** Query do pobierania aktualnego użytkownika.

---

## ETAP 8: Implementacja DTOs i Mappings

### Zadanie 8.1: DTOs

**UserDto.cs:**
```csharp
namespace PortalForge.Application.Auth.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}
```

**AuthResponseDto.cs:**
```csharp
namespace PortalForge.Application.Auth.DTOs;

public class AuthResponseDto
{
    public UserDto User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

**LoginRequestDto.cs:**
```csharp
namespace PortalForge.Application.Auth.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

**RegisterRequestDto.cs:**
```csharp
namespace PortalForge.Application.Auth.DTOs;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

**ResetPasswordRequestDto.cs:**
```csharp
namespace PortalForge.Application.Auth.DTOs;

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}
```

### Zadanie 8.2: AutoMapper Profile

**AuthMappingProfile.cs:**
```csharp
using AutoMapper;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<LoginRequestDto, LoginCommand>();
        CreateMap<RegisterRequestDto, RegisterCommand>();
        CreateMap<ResetPasswordRequestDto, ResetPasswordCommand>();
    }
}
```

**Output:** Wszystkie DTOs i profile mappingów.

---

## ETAP 9: Implementacja API Controllers

### Zadanie 9.1: AuthController

**AuthController.cs:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using AutoMapper;
using PortalForge.Application.Auth.Commands.Login;
using PortalForge.Application.Auth.Commands.Register;
using PortalForge.Application.Auth.Commands.Logout;
using PortalForge.Application.Auth.Commands.ResetPassword;
using PortalForge.Application.Auth.Queries.GetCurrentUser;
using PortalForge.Application.Auth.DTOs;

namespace PortalForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AuthController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Logowanie użytkownika
    /// </summary>
    /// <param name="request">Dane logowania (email i hasło)</param>
    /// <returns>Dane użytkownika i tokeny autoryzacyjne</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequestDto request)
    {
        var command = _mapper.Map<LoginCommand>(request);
        var result = await _mediator.Send(command);

        // Ustaw HTTP-only cookies
        SetAuthCookies(result.AccessToken, result.RefreshToken);

        return Ok(result);
    }

    /// <summary>
    /// Rejestracja nowego użytkownika
    /// </summary>
    /// <param name="request">Dane rejestracyjne</param>
    /// <returns>Dane użytkownika i tokeny autoryzacyjne</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterRequestDto request)
    {
        var command = _mapper.Map<RegisterCommand>(request);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetCurrentUser), result);
    }

    /// <summary>
    /// Wylogowanie użytkownika
    /// </summary>
    /// <returns>Brak zawartości</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutCommand());

        // Usuń cookies
        ClearAuthCookies();

        return NoContent();
    }

    /// <summary>
    /// Resetowanie hasła - wysyła email z linkiem
    /// </summary>
    /// <param name="request">Email użytkownika</param>
    /// <returns>Brak zawartości</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto request)
    {
        await _mediator.Send(new ResetPasswordCommand { Email = request.Email });
        return NoContent();
    }

    /// <summary>
    /// Pobranie danych aktualnie zalogowanego użytkownika
    /// </summary>
    /// <returns>Dane użytkownika</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery());
        return Ok(result);
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        };

        Response.Cookies.Append("sb-access-token", accessToken, cookieOptions);

        cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
        Response.Cookies.Append("sb-refresh-token", refreshToken, cookieOptions);
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("sb-access-token");
        Response.Cookies.Delete("sb-refresh-token");
    }
}
```

**Output:** Pełny controller z wszystkimi endpointami i dokumentacją XML.

---

## ETAP 10: Implementacja Middleware

### Zadanie 10.1: AuthenticationMiddleware

**AuthenticationMiddleware.cs:**
```csharp
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Api.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISupabaseAuthService authService)
    {
        var accessToken = context.Request.Cookies["sb-access-token"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            try
            {
                var user = await authService.GetUserAsync(accessToken);

                if (user != null)
                {
                    context.Items["User"] = user;
                    context.Items["AccessToken"] = accessToken;

                    _logger.LogDebug("User authenticated: {UserId}", user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate access token");
            }
        }

        await _next(context);
    }
}
```

### Zadanie 10.2: CurrentUserService

**ICurrentUserService.cs:**
```csharp
namespace PortalForge.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? AccessToken { get; }
    bool IsAuthenticated { get; }
}
```

**CurrentUserService.cs:**
```csharp
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            return user?.Id;
        }
    }

    public string? Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            return user?.Email;
        }
    }

    public string? AccessToken
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Items["AccessToken"] as string;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;
}
```

### Zadanie 10.3: Rejestracja Middleware

W `Program.cs`:
```csharp
// Add to services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add to pipeline (BEFORE app.UseAuthorization())
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();
```

**Output:** Middleware do zarządzania autoryzacją w pipeline.

---

## ETAP 11: Testy jednostkowe

### Zadanie 11.1: Testy LoginCommandHandler

**LoginCommandHandlerTests.cs:**
```csharp
using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Auth.Commands.Login;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Tests.Unit.Application.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<ISupabaseAuthService> _authServiceMock;
    private readonly Mock<IUnifiedValidatorService> _validatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authServiceMock = new Mock<ISupabaseAuthService>();
        _validatorMock = new Mock<IUnifiedValidatorService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _authServiceMock.Object,
            _validatorMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var authResult = new AuthResult
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                Role = UserRole.Employee
            },
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        };

        _authServiceMock
            .Setup(x => x.SignInWithPasswordAsync(command.Email, command.Password))
            .ReturnsAsync(authResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _authServiceMock.Verify(
            x => x.SignInWithPasswordAsync(command.Email, command.Password),
            Times.Once);
        _validatorMock.Verify(
            x => x.ValidateAsync(command),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _authServiceMock
            .Setup(x => x.SignInWithPasswordAsync(command.Email, command.Password))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials");
    }
}
```

### Zadanie 11.2: Testy LoginCommandValidator

**LoginCommandValidatorTests.cs:**
```csharp
using Xunit;
using FluentAssertions;
using PortalForge.Application.Auth.Commands.Login;
using PortalForge.Application.Auth.Commands.Login.Validation;

namespace PortalForge.Tests.Unit.Application.Auth.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    public void Validate_InvalidEmail_FailsValidation(string email)
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = email,
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_InvalidPassword_FailsValidation(string password)
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginCommand.Password));
    }
}
```

### Zadanie 11.3: Testy RegisterCommandValidator

**RegisterCommandValidatorTests.cs:**
```csharp
using Xunit;
using Moq;
using FluentAssertions;
using PortalForge.Application.Auth.Commands.Register;
using PortalForge.Application.Auth.Commands.Register.Validation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Tests.Unit.Application.Auth.Validators;

public class RegisterCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new RegisterCommandValidator(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_PasswordMismatch_FailsValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.ConfirmPassword));
    }

    [Theory]
    [InlineData("NoNumber!")]
    [InlineData("nonumber123!")]
    [InlineData("NoSpecialChar123")]
    [InlineData("short1!")]
    public async Task Validate_WeakPassword_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(RegisterCommand.Password));
    }
}
```

### Zadanie 11.4: Testy EmailService

**EmailServiceTests.cs:**
```csharp
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PortalForge.Infrastructure.Email;
using PortalForge.Infrastructure.Email.Models;

namespace PortalForge.Tests.Unit.Infrastructure.Email;

public class EmailServiceTests
{
    private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
        _loggerMock = new Mock<ILogger<EmailService>>();

        var emailSettings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            SmtpPort = 587,
            UseTLS = true,
            Username = "test@test.com",
            Password = "password",
            FromEmail = "test@test.com",
            FromName = "Test"
        };

        _emailSettingsMock.Setup(x => x.Value).Returns(emailSettings);

        _emailService = new EmailService(_emailSettingsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_ValidEmail_LogsInformation()
    {
        // Arrange
        var email = "user@example.com";
        var link = "https://test.com/verify?token=xyz";

        // Act
        // Note: Ten test wymaga mock SMTP lub integration test
        // Tutaj testujemy tylko logikę

        // Assert
        // Sprawdź czy metoda nie rzuca wyjątku
        var act = () => _emailService.SendVerificationEmailAsync(email, link);
        await act.Should().NotThrowAsync();
    }
}
```

**Output:** Kompletne testy jednostkowe z coverage > 70%.

---

## ETAP 12: Dokumentacja i finalizacja

### Zadanie 12.1: Decision o implementacji

Stwórz `.ai/decisions/003-auth-backend-implementation.md`:
```markdown
# ADR 003: Implementacja Backendu Autoryzacji

## Status
Zaakceptowane - 2025-01-16

## Kontekst
Implementacja backendu autoryzacji z integracją Supabase Auth i własnym systemem email.

## Decyzja

### Architektura walidacji
- IUnifiedValidatorService wzorowane na WatchdogAPI
- Automatyczna rejestracja validatorów przez reflection (ValidatorExtension)
- Validators w folderach `Validation/` obok Handlers
- FluentValidation z async validation dla złożonych reguł

### Email Service
- Biblioteka: MailKit (najbardziej dojrzała, używana przez Microsoft)
- Lokalizacja: Infrastructure/Email/
- 3 szablony HTML jako embedded resources:
  1. VerificationEmail.html - powitanie + link weryfikacyjny
  2. PasswordResetEmail.html - reset hasła
  3. PasswordChangedEmail.html - potwierdzenie zmiany
- SMTP: smtp.zenbox.pl:587 (TLS)

### Supabase Integration
- Supabase Auth dla generowania tokenów JWT
- Własny SMTP dla wysyłki emaili (pełna kontrola + brak limitów)
- HTTP-only cookies dla przechowywania tokenów
- Automatyczne odświeżanie tokenów przez middleware

### CQRS Implementation
- Commands: Login, Register, Logout, ResetPassword
- Queries: GetCurrentUser
- Każdy handler używa IUnifiedValidatorService
- Structured logging z Serilog

### Struktura projektu testowego
- Osobny projekt PortalForge.Tests
- Podział na Unit i Integration
- xUnit + FluentAssertions + Moq
- InMemory database dla integration tests
- TestDataBuilder pattern dla złożonych obiektów

### Security
- HTTP-only cookies dla tokenów
- CSRF protection
- Rate limiting na endpointach auth
- Secure cookies (tylko HTTPS w produkcji)
- Password strength validation:
  - Min. 8 znaków
  - Wielka litera
  - Mała litera
  - Cyfra
  - Znak specjalny

## Konsekwencje

### Pozytywne
- Czysty podział odpowiedzialności (Clean Architecture)
- Automatyczna walidacja bez boilerplate code
- Łatwe testowanie (wszystkie zależności przez DI)
- Pełna kontrola nad emailami i ich wyglądem
- Bezpieczne przechowywanie tokenów
- Izolowane testy jednostkowe
- Spójna dokumentacja XML dla API

### Negatywne
- Dodatkowa złożoność EmailService
- Konieczność zarządzania SMTP credentials
- Więcej projektów w solution (Tests)
- Dependency na external SMTP server

## Alternatywy rozważane

1. **FluentValidation Pipeline Behavior vs IUnifiedValidatorService**
   - Wybraliśmy IUnifiedValidatorService (wzór z WatchdogAPI)
   - Powód: Większa elastyczność, explicit validation calls

2. **Supabase email vs własny SMTP**
   - Wybraliśmy własny SMTP
   - Powód: Pełna kontrola, brak limitów, custom templates

3. **JWT w localStorage vs HTTP-only cookies**
   - Wybraliśmy HTTP-only cookies
   - Powód: Bezpieczeństwo (XSS protection)
```

### Zadanie 12.2: Zaktualizuj progress w PRD

Dodaj do `.ai/prd.md` lub stwórz `.ai/progress.md`:
```markdown
## Progress Implementacji - PortalForge Backend

### ✅ Moduł Autoryzacji (US-001, US-002)

#### Infrastruktura
- [x] Struktura projektu zgodna z Clean Architecture
- [x] IUnifiedValidatorService (wzór WatchdogAPI)
- [x] ValidatorExtension - automatyczna rejestracja
- [x] EmailService z MailKit
- [x] 3 szablony HTML emaili (embedded resources)
- [x] Konfiguracja SMTP

#### Baza danych
- [x] Encja User w Domain Layer
- [x] UserConfiguration dla EF Core
- [x] Migracje EF Core
- [x] Zastosowanie migracji w Supabase PostgreSQL

#### Integracja Supabase
- [x] ISupabaseAuthService interface
- [x] SupabaseAuthService implementation
- [x] Integracja z Supabase C# SDK
- [x] Konfiguracja Supabase URL i Keys

#### CQRS Commands
- [x] LoginCommand + Handler + Validator
- [x] RegisterCommand + Handler + Validator
- [x] LogoutCommand + Handler
- [x] ResetPasswordCommand + Handler + Validator

#### CQRS Queries
- [x] GetCurrentUserQuery + Handler

#### DTOs i Mappings
- [x] UserDto, AuthResponseDto
- [x] LoginRequestDto, RegisterRequestDto, ResetPasswordRequestDto
- [x] AuthMappingProfile (AutoMapper)

#### API Controllers
- [x] AuthController z pełną dokumentacją XML
- [x] Endpointy: POST /login, /register, /logout, /reset-password
- [x] Endpoint: GET /me
- [x] HTTP-only cookies dla tokenów

#### Middleware
- [x] AuthenticationMiddleware
- [x] CurrentUserService
- [x] Rejestracja w pipeline

#### Testy
- [x] Projekt PortalForge.Tests
- [x] Testy LoginCommandHandler
- [x] Testy LoginCommandValidator
- [x] Testy RegisterCommandValidator
- [x] Testy EmailService
- [x] TestDataBuilder helpers

#### Dokumentacja
- [x] ADR 002: Struktura backendu
- [x] ADR 003: Implementacja autoryzacji
- [x] XML documentation w controllers
- [x] README dla modułu Auth
- [x] Progress update w PRD

### 📊 Statystyki
- **Commands**: 4 (Login, Register, Logout, ResetPassword)
- **Queries**: 1 (GetCurrentUser)
- **Validators**: 4
- **Tests**: 5+ (unit + integration)
- **Coverage**: >70%
- **Endpointy API**: 5
- **Email templates**: 3

### 🔜 Następne kroki
- [ ] Implementacja middleware auth dla pozostałych endpointów
- [ ] Dodanie rate limiting
- [ ] Implementacja refresh token endpoint
- [ ] Integracja z frontendem
- [ ] Testy E2E z Playwright
```

### Zadanie 12.3: Stwórz README dla modułu Auth

Dodaj `backend/PortalForge.Application/Auth/README.md`:
```markdown
# Moduł Autoryzacji - PortalForge

## Architektura

Moduł autoryzacji implementuje:
- **Clean Architecture** z wyraźnym podziałem warstw
- **CQRS** z MediatR dla separacji commands i queries
- **FluentValidation** z IUnifiedValidatorService (wzór WatchdogAPI)
- **Supabase Auth** dla tokenów JWT
- **MailKit** dla wysyłki emaili z własnymi szablonami HTML

## Endpointy API

### POST /api/auth/login
Logowanie użytkownika do systemu.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:** 200 OK
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "role": "Employee",
    "isEmailVerified": true
  },
  "accessToken": "eyJhbGc...",
  "refreshToken": "refresh-token..."
}
```

**Cookies ustawiane:**
- `sb-access-token` (HttpOnly, Secure, 1h)
- `sb-refresh-token` (HttpOnly, Secure, 7 dni)

### POST /api/auth/register
Rejestracja nowego użytkownika.

**Request:**
```json
{
  "email": "newuser@example.com",
  "password": "StrongPass123!",
  "confirmPassword": "StrongPass123!"
}
```

**Response:** 201 Created
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "newuser@example.com",
    "role": "Employee",
    "isEmailVerified": false
  },
  "accessToken": "eyJhbGc...",
  "refreshToken": "refresh-token..."
}
```

**Automatyczne akcje:**
- Email weryfikacyjny wysyłany na podany adres
- User dodawany do bazy Supabase Auth
- User dodawany do lokalnej bazy PostgreSQL

### POST /api/auth/logout
Wylogowanie użytkownika (wymaga autoryzacji).

**Headers:**
```
Cookie: sb-access-token=eyJhbGc...
```

**Response:** 204 No Content

**Automatyczne akcje:**
- Sesja unieważniana w Supabase Auth
- Cookies usuwane

### POST /api/auth/reset-password
Żądanie resetu hasła.

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response:** 204 No Content

**Automatyczne akcje:**
- Email z linkiem resetującym wysyłany na podany adres
- Token ważny przez 1 godzinę

### GET /api/auth/me
Pobranie danych aktualnie zalogowanego użytkownika (wymaga autoryzacji).

**Headers:**
```
Cookie: sb-access-token=eyJhbGc...
```

**Response:** 200 OK
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "role": "Employee",
  "isEmailVerified": true
}
```

## Walidacja

### Walidacja hasła (RegisterCommand)
- Minimum 8 znaków
- Przynajmniej jedna wielka litera
- Przynajmniej jedna mała litera
- Przynajmniej jedna cyfra
- Przynajmniej jeden znak specjalny

### Walidacja email
- Format email (RFC 5322)
- Maksymalnie 255 znaków
- Unikalność (sprawdzana w bazie)

## Bezpieczeństwo

### Przechowywanie tokenów
- Tokeny JWT w **HTTP-only cookies**
- **Secure flag** (tylko HTTPS w produkcji)
- **SameSite=Strict** (ochrona CSRF)

### Sesje
- Access token: 1 godzina
- Refresh token: 7 dni
- Automatyczne odświeżanie przez middleware

### Rate limiting
- Login: 5 prób / 5 minut na IP
- Register: 3 rejestracje / godzinę na IP
- Reset password: 3 żądania / godzinę na email

## Email templates

### 1. VerificationEmail.html
Wysyłany po rejestracji. Zawiera:
- Link weryfikacyjny (ważny 1h)
- Instrukcje aktywacji konta

### 2. PasswordResetEmail.html
Wysyłany po żądaniu resetu. Zawiera:
- Link do ustawienia nowego hasła (ważny 1h)
- Informację o wylogowaniu wszystkich sesji

### 3. PasswordChangedEmail.html
Wysyłany po zmianie hasła. Zawiera:
- Potwierdzenie zmiany hasła
- Datę i czas zmiany
- Instrukcje w przypadku nieautoryzowanej zmiany

## Struktura kodu

```
Auth/
├── Commands/
│   ├── Login/
│   │   ├── LoginCommand.cs
│   │   ├── LoginCommandHandler.cs
│   │   └── Validation/
│   │       └── LoginCommandValidator.cs
│   ├── Register/
│   ├── Logout/
│   └── ResetPassword/
├── Queries/
│   └── GetCurrentUser/
└── DTOs/
```

## Dependency Injection

Wszystkie serwisy zarejestrowane w DI:
- `IUnifiedValidatorService` → `UnifiedValidatorService` (Scoped)
- `ISupabaseAuthService` → `SupabaseAuthService` (Scoped)
- `IEmailService` → `EmailService` (Scoped)
- `ICurrentUserService` → `CurrentUserService` (Scoped)
- Wszystkie validators → automatycznie przez `ValidatorExtension`

## Testowanie

Uruchom testy:
```bash
cd PortalForge.Tests
dotnet test
```

Coverage:
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura
```

## Troubleshooting

### Problem: "Email already exists"
- Sprawdź czy użytkownik nie istnieje w Supabase Dashboard
- Sprawdź bazę PostgreSQL: `SELECT * FROM public."Users" WHERE "Email" = 'email@example.com'`

### Problem: "Failed to send email"
- Sprawdź konfigurację SMTP w appsettings.json
- Sprawdź logi: `dotnet run --project PortalForge.Api --verbosity detailed`
- Sprawdź czy port 587 nie jest zablokowany

### Problem: "Invalid or expired token"
- Sprawdź czy access_token nie wygasł (1h lifetime)
- Sprawdź czy cookies są ustawiane poprawnie
- Sprawdź middleware order w Program.cs

## Konfiguracja

**Development:**
```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "eyJhbGc..."
  },
  "EmailSettings": {
    "SmtpServer": "smtp.zenbox.pl",
    "SmtpPort": 587,
    "UseTLS": true,
    "Username": "portalforge@krablab.pl",
    "Password": "***",
    "FromEmail": "portalforge@krablab.pl",
    "FromName": "PortalForge"
  }
}
```

**Production:**
Użyj User Secrets lub Environment Variables!
```

**Output:** Kompletna dokumentacja modułu autoryzacji.

---

## Checklist przed zakończeniem

Upewnij się, że wszystkie poniższe punkty zostały wykonane:

### Struktura projektu
- [ ] Wszystkie foldery zgodne z Clean Architecture
- [ ] Projekt PortalForge.Tests utworzony i skonfigurowany
- [ ] Wszystkie zależności między projektami prawidłowe

### Kod
- [ ] IUnifiedValidatorService zaimplementowany
- [ ] ValidatorExtension z automatyczną rejestracją
- [ ] EmailService z 3 szablonami HTML
- [ ] Wszystkie Commands mają Validators w folderach Validation/
- [ ] Wszystkie DTOs mają mappings
- [ ] Controller ma XML documentation
- [ ] Middleware zarejestrowany w Program.cs
- [ ] CurrentUserService zarejestrowany w DI
- [ ] Wszystkie serwisy zarejestrowane w DI

### Baza danych
- [ ] Migracje EF Core wygenerowane
- [ ] Migracje zastosowane w Supabase PostgreSQL
- [ ] Encje prawidłowo skonfigurowane

### Email
- [ ] SMTP skonfigurowany w appsettings
- [ ] Szablony HTML jako embedded resources
- [ ] EmailService wysyła emaile poprawnie

### Testy
- [ ] Testy dla handlers
- [ ] Testy dla validators
- [ ] Testy dla EmailService
- [ ] Coverage > 70%
- [ ] Wszystkie testy przechodzą

### Dokumentacja
- [ ] ADR 002 zapisany
- [ ] ADR 003 zapisany
- [ ] Progress w PRD zaktualizowany
- [ ] README dla modułu Auth utworzony

### Kompilacja
- [ ] Zero błędów kompilacji
- [ ] Zero warnings ReSharper/Rider
- [ ] Kod zgodny z .claude/backend.md

### Bezpieczeństwo
- [ ] Hasła hashowane przez Supabase Auth
- [ ] Tokeny w HTTP-only cookies
- [ ] SMTP credentials w appsettings (development only!)
- [ ] Production używa User Secrets

---

## Kolejność wykonania zadań

1. **ETAP 0** - Analiza WatchdogAPI (30 min)
2. **ETAP 1** - Struktura projektu (45 min)
3. **ETAP 2** - IUnifiedValidatorService (30 min)
4. **ETAP 3** - EmailService (90 min)
5. **ETAP 4** - Entity Framework (45 min)
6. **ETAP 5** - Supabase Auth (60 min)
7. **ETAP 6** - CQRS Commands (120 min)
8. **ETAP 7** - CQRS Queries (30 min)
9. **ETAP 8** - DTOs i Mappings (30 min)
10. **ETAP 9** - Controllers (45 min)
11. **ETAP 10** - Middleware (30 min)
12. **ETAP 11** - Testy (90 min)
13. **ETAP 12** - Dokumentacja (45 min)

**Szacowany czas całkowity: 10-12 godzin**

---

## Finalny output

Po zakończeniu pracy dostarcz:

1. ✅ Pełną strukturę projektu backend dla autoryzacji
2. ✅ IUnifiedValidatorService (wzór WatchdogAPI)
3. ✅ EmailService z MailKit + 3 szablony HTML
4. ✅ Wszystkie encje EF Core z migracjami
5. ✅ Pełna integracja z Supabase Auth
6. ✅ Wszystkie Commands i Queries z CQRS
7. ✅ Validators w folderach Validation/ obok Handlers
8. ✅ DTOs i AutoMapper profiles
9. ✅ AuthController z pełną dokumentacją XML
10. ✅ AuthenticationMiddleware i CurrentUserService
11. ✅ Projekt testowy PortalForge.Tests
12. ✅ Testy jednostkowe (coverage > 70%)
13. ✅ Decision documents (ADR 002, 003)
14. ✅ Zaktualizowany progress w PRD
15. ✅ README dla modułu Auth

---

## Kontakt SMTP (do usunięcia po implementacji)

**DANE SMTP - TYLKO DLA DEVELOPMENTU!**
```
Server: smtp.zenbox.pl
Port: 587 (TLS)
Username: portalforge@krablab.pl
Password: gst48fz@$Bvt
```

**⚠️ UWAGA:** Te dane są tylko dla development! W produkcji użyj User Secrets lub Environment Variables!

---

Powodzenia z implementacją! 🚀

## Wsparcie

W razie pytań lub problemów:
1. Sprawdź dokumentację w README
2. Sprawdź decision documents w `.ai/decisions/`
3. Sprawdź testy jednostkowe jako przykłady użycia
4. Sprawdź logi aplikacji
