# CLAUDE.md

## Wymagane Skille - PRZECZYTAJ PRZED PRACA

**OBOWIAZKOWE** - stosuj przy kazdej pracy z kodem:
- ~/.claude/skills/dotnet-enterprise.md - standardy .NET (backend)
- ~/.claude/skills/vue-enterprise.md - standardy Vue.js (frontend)

**UZUPELNIENIE PROJEKTOWE** - specyficzne dla PortalForge:
- .claude/backend.md - ITransactionalRequest, konfiguracja bez hardkodowania
- .claude/frontend.md - Nuxt 3 specifics, Supabase integration
- .claude/testing.md - konwencje testow xUnit/Vitest/Playwright

**WORKFLOW** - dla kazdego nowego zadania stosuj pelny przeplyw opisany nizej.

**IMPORTANT INSTRUCTIONS:**
- NEVER add comments like Generated with Claude Code or similar attribution
- NEVER add Co-Authored-By headers or similar AI metadata
- Focus on clean, maintainable, well-documented code
- Prioritize code quality over speed

---

## Project Overview

**PortalForge** to wewnetrzny portal intranetowy dla organizacji 200+ pracownikow.

**Current Phase**: MVP Development - Phase 1 (Foundation)
**Repository**: https://github.com/Vesperino/PortalForge.git

---

## Tech Stack

| Warstwa | Technologia |
|---------|-------------|
| Backend | .NET 8.0, ASP.NET Core, EF Core, PostgreSQL (Supabase) |
| Architecture | Clean Architecture + CQRS (MediatR) |
| Frontend | Nuxt 3, Vue 3, TypeScript, Pinia, Tailwind CSS |
| Auth | Supabase Auth |
| Testing | xUnit + FluentAssertions + Moq, Vitest + Playwright |
| Logging | Serilog |
| Validation | FluentValidation |

---

## Komendy

### Backend (.NET)

| Komenda | Katalog | Opis |
|---------|---------|------|
| dotnet restore | backend/PortalForge.Api | Restore dependencies |
| dotnet run | backend/PortalForge.Api | Start serwera |
| dotnet watch run | backend/PortalForge.Api | Hot reload |
| dotnet test | backend/ | Uruchom testy |
| dotnet ef migrations add Name | backend/PortalForge.Api | Nowa migracja |
| dotnet ef database update | backend/PortalForge.Api | Aplikuj migracje |

### Frontend (Nuxt)

| Komenda | Katalog | Opis |
|---------|---------|------|
| npm install | frontend | Install dependencies |
| npm run dev | frontend | Start dev server |
| npm run build | frontend | Production build |
| npm run test | frontend | Unit tests (Vitest) |
| npm run test:e2e | frontend | E2E tests (Playwright) |
| npm run lint | frontend | ESLint check |

---

## Dostepne Skille (Slash Commands)

| Skill | Opis | Kiedy uzywac |
|-------|------|--------------|
| /new-feature | Planowanie i implementacja nowej funkcji | Nowe feature |
| /debug | Analiza i rozwiazywanie problemow | Bledy, bugi |
| /test | Generowanie testow | Nowy kod |
| /review-code | Code review wedlug standardow | Przed PR/commit |
| /deploy | Deployment do staging/production | Release |
| /update-docs | Aktualizacja dokumentacji | Po zakonczeniu feature |

---

## Workflow - OBOWIAZKOWY

1. ANALIZA WYMAGAN
   - Zrozum wymagania zadania
   - Sprawdz czy jest w scope MVP (.ai/prd.md)
   - Zaproponuj 2-3 podejscia jesli potrzeba

2. PLANOWANIE (/new-feature dla nowych funkcji)
   - Rozpisz zadania na male kroki
   - Uzyj TodoWrite do sledzenia postepu
   - OZNACZ ZADANIA ROWNOLEGLE [PARALLEL]

3. GIT WORKFLOW
   - Utworz branch: feature/nazwa lub fix/nazwa
   - Conventional commits

4. IMPLEMENTACJA - WYBIERZ TRYB:
   - A) SEKWENCYJNIE - dla zadan z zaleznosciami
   - B) ROWNOLEGLE (Task tool) - dla 2+ niezaleznych zadan

   Kazdy krok stosuje:
   - dotnet-enterprise.md dla kodu .NET
   - vue-enterprise.md dla kodu Vue/Nuxt
   - /debug dla problemow
   - /test dla nowej logiki

5. WERYFIKACJA
   - dotnet build - ZERO WARNINGS
   - dotnet test - WSZYSTKIE PRZECHODZA
   - npm run build - ZERO BLEDOW
   - npm run lint - ZERO BLEDOW

6. CODE REVIEW (/review-code)
   - Przejrzyj wlasne zmiany
   - Sprawdz zgodnosc ze skillami

7. FINALIZACJA
   - Commit z conventional message
   - Utworz PR lub merge
   - /update-docs jesli potrzeba

### Kiedy uzywac rownolegych agentow (Task tool)

| Scenariusz | Strategia |
|------------|-----------|
| Feature fullstack (API + UI) | 2 agenty: backend + frontend rownolegle |
| Wiele niezaleznych komponentow | Agent per komponent |
| Testy + implementacja | Agent na testy, agent na kod |
| Refactor wielu modulow | Agent per modul |

---

## Checklista przed Commit/PR

### Backend (z dotnet-enterprise.md)
- dotnet build - zero warnings
- dotnet test - 100% passing
- Kazda klasa w osobnym pliku
- SOLID przestrzegane
- Brak N+1 (sprawdz Include)
- CancellationToken propagowany
- Result pattern lub custom exceptions
- Brak hardkodowanych wartosci

### Frontend (z vue-enterprise.md)
- npm run build - zero errors
- npm run lint - zero errors
- script setup lang=ts wszedzie
- Brak any w TypeScript
- Max 200 linii na komponent
- Composables z prefixem use
- data-testid dla interaktywnych elementow

---

## Do NOT

- Do NOT edit appsettings.json - use appsettings.Development.json
- Do NOT commit secrets, API keys, passwords
- Do NOT commit node_modules/, bin/, obj/
- Do NOT force push to main
- Do NOT skip migrations
- Do NOT use any type in TypeScript
- Do NOT bypass authentication/authorization
- Do NOT use async void (except event handlers)
- Do NOT block async with .Result or .Wait()
- Do NOT create commands/queries without validators
- Do NOT access database directly from controllers
- Do NOT put business logic in controllers
- Do NOT hardcode paths, URLs, or configuration values
- Do NOT use Options API, Mixins, or Event Bus (Vue)

---

## MVP Scope

### IN SCOPE
- User authentication (Supabase Auth)
- Role-based access (Admin, Manager, HR, Marketing, Employee)
- Organizational structure management
- Calendar of company events
- News system
- User activity monitoring
- CSV/Excel import of users
- PDF/Excel export of org structure

### OUT OF SCOPE (Future)
- Request system with workflows
- Document management and versioning
- Active Directory/LDAP integration
- Dedicated admin panel
- Email/Push notifications
- Full-text search
- Internal chat/messenger
- External API integrations

---

## Key Business Rules

1. Kazdy pracownik musi miec: First name, Last name, Email, Department, Position, Supervisor
2. Tylko Admin/HR moze importowac przez CSV/Excel
3. Tylko Admin/HR/Marketing moze tworzyc news i events
4. Managers moga edytowac tylko strukture swojego dzialu
5. Events archiwizowane po 1 roku
6. Sessions wygasaja po 8h nieaktywnosci
7. Passwords hashowane bcrypt
8. API responses ponizej 500ms dla 95% requestow

---

## Debugging Tips

| Problem | Rozwiazanie |
|---------|-------------|
| N+1 queries | Dodaj .Include(), sprawdz .ToQueryString() |
| Validation errors | Sprawdz FluentValidation + UnifiedValidatorService |
| Auth errors | Sprawdz Supabase token, JWT config |
| Vue reactivity | Uzywaj storeToRefs() dla Pinia state |
| TypeScript errors | npm run lint dla pelnego raportu |
| EF migrations | Sprawdz czy DbContext ma DbSet |

---

## Supabase Configuration

Project: https://mqowlgphivdosieakzjb.supabase.co

Environment Variables:
- Frontend: frontend/.env (gitignored)
  - NUXT_PUBLIC_SUPABASE_URL
  - NUXT_PUBLIC_SUPABASE_KEY (anon key)
- Backend: backend/PortalForge.Api/.env (gitignored)
  - SUPABASE_URL
  - SUPABASE_SERVICE_ROLE_KEY (secret!)
  - CONNECTION_STRING

SECURITY: Nigdy nie commituj .env files!

---

## References

- Project Docs: .ai/prd.md, .ai/tech-stack.md
- .NET: https://learn.microsoft.com/en-us/dotnet/
- Nuxt: https://nuxt.com/docs
- Supabase: https://supabase.com/docs
- Tailwind: https://tailwindcss.com/docs
- MediatR: https://github.com/jbogard/MediatR/wiki
- FluentValidation: https://docs.fluentvalidation.net/

---

Last Updated: 2026-01-01
Version: 3.0.0
